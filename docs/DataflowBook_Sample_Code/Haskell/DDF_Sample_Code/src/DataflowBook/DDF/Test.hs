module DataflowBook.DDF.Test where

import Control.Concurrent
import Control.Monad
import Data.List
import Data.Maybe
import DataflowBook.DDF.GeneralTypes
import DataflowBook.DDF.PrimitiveNodeClass
import DataflowBook.DDF.NodeClass
import DataflowBook.DDF.NodeObject

{-
	- Does not retain state from stop to start
		- Can be fixed by storing state in global mutable var
	- Does not check that it can send tokens before a node performs its action
		- Can only be fixed with chans that allow you to query fullness
	- Does not allow recursive nodes - no lower node may contain a compound node used at a higher level
	- Does not allow circular references - same as recursive nodes

	TODO
		- start should aways call stop to ensure no memory leak with functions
		- connection should restart if already running
		- add disconnect
		- add hasPort :: Boolean
		- verify system first

	Possible Improvements:
		- Setup chan to send debug info 

-}

nodeCounter = PrimitiveClass "counter" $ PrimitiveNodeClass [] ["out"] 
	(\(_,state) -> do
		let val = case (lookup "val" state) of
			Nothing -> 0
			Just n -> n
		let new_n = val+1
		threadDelay 5000000
		return ([("out",I new_n)],[("val",new_n)])
	)

nodePrinter prefix class_name = PrimitiveClass class_name $ PrimitiveNodeClass ["in"] []
	(\((portid,token),state) -> do
		let I n = token
		putStrLn $ concat [prefix,show n]
		return ([],state)
	)



nodeTest1 = CompoundClass "test1" $ CompoundNodeClass [] [] [("ctr","counter"),("ptr","printer")] [(("ctr","out"),("ptr","in"))]

test1 = do
	maybe_obj <- mkSystem [nodePrinter "" "printer",nodeCounter,nodeTest1] ("root","test1")
	objectStart (fromJust maybe_obj)

nodeAnd = PrimitiveClass "and" $ PrimitiveNodeClass ["a","b"] ["out"]
	(\((portid,I val),state) -> do
--		putStr (concat ["and: ", show portid, " ", show val])
		let a = case (lookup "a" state) of
			Nothing -> 0
			Just n -> n
		let b = case (lookup "b" state) of
			Nothing -> 0
			Just n -> n
		let (a',b') = case portid of
			"a" -> (val,b)
			"b" -> (a,val)
			_ -> (a,b)
		let out = case (a',b') of
			(0,0) -> 0
			(0,1) -> 0
			(1,0) -> 0
			(1,1) -> 1
		let new_state = [("a",a'),("b",b')]
		return ([("out",I out)],new_state)
	)

nodeXor = PrimitiveClass "xor" $ PrimitiveNodeClass ["a","b"] ["out"]
	(\((portid,I val),state) -> do
--		putStr (concat ["xor: ", show portid, " ", show val])
		let a = case (lookup "a" state) of
			Nothing -> 0
			Just n -> n
		let b = case (lookup "b" state) of
			Nothing -> 0
			Just n -> n
		let (a',b') = case portid of
			"a" -> (val,b)
			"b" -> (a,val)
			_ -> (a,b)
		let out = case (a',b') of
			(0,0) -> 0
			(0,1) -> 1
			(1,0) -> 1
			(1,1) -> 0
		let new_state = [("a",a'),("b",b')]
		return ([("out",I out)],new_state)
	)
	
nodeOr = PrimitiveClass "or" $ PrimitiveNodeClass ["a","b"] ["out"]
	(\((portid,I val),state) -> do
		let a = case (lookup "a" state) of
			Nothing -> 0
			Just n -> n
		let b = case (lookup "b" state) of
			Nothing -> 0
			Just n -> n
		let (a',b') = case portid of
			"a" -> (val,b)
			"b" -> (a,val)
			_ -> (a,b)
		let out = case (a',b') of
			(0,0) -> 0
			(0,1) -> 1
			(1,0) -> 1
			(1,1) -> 1
		let new_state = [("a",a'),("b",b')]
		return ([("out",I out)],new_state)
	)

nodeNot = PrimitiveClass "not" $ PrimitiveNodeClass ["a"] ["out"]
	(\((portid,I val),state) -> do
		let a = case (lookup "a" state) of
			Nothing -> 0
			Just n -> n
		let a' = case portid of
			"a" -> val
			_ -> a
		let out = case a' of
			0 -> 1
			1 -> 0
		let new_state = [("a",a')]
		return ([("out",I out)],new_state)
	)
nodeSpliter2 = PrimitiveClass "spliter2" $ PrimitiveNodeClass ["in"] ["1","2"]
	(\((_,token),state) -> do
		return ([("1",token),("2",token)],state)
	)

-- wrong ideal: can't set value per object
nodeOneShot val = PrimitiveClass "oneshot" $ PrimitiveNodeClass [] ["out"]
	(\(_,state) -> 
		case (lookup "shot" state) of
			Nothing -> do
				let new_state = [("shot",1)]
				return ([("out",I val)], new_state)
			Just _ -> return ([],state)
	)

nodeHalfAdder = CompoundClass "halfadder" $ CompoundNodeClass 
	-- Inputs
	[("a",("split_a","a")) 
	,("b",("split_b","b"))
	]
	-- Outputs 
	[("sum",("xor1","out"))
	,("carry",("and1","out"))
	]
	-- Names
	[("and1","and")
	,("xor1","xor")
	,("split_a","spliter2")
	,("split_b","spliter2")
	]
	-- Arcs
	[(("split_a","1"),("xor1","a"))
	,(("split_b","1"),("xor1","b"))
	,(("split_a","2"),("and1","a"))
	,(("split_b","2"),("and1","b"))
	]

nodeFullAdder = CompoundClass "fulladder" $ CompoundNodeClass
	-- Inputs
	[("a",("ha1","a"))
	,("b",("ha1","b"))
	,("carry_in",("ha2","b"))
	]
	-- Outputs
	[("sum",("ha2","sum"))
	,("carry_out",("or1","out"))
	]
	-- Names
	[("ha1","halfadder")
	,("ha2","halfadder")
	,("or1","or")
	]
	-- Arcs
	[(("ha1","sum"),("ha2","a"))
	,(("ha1","carry"),("or1","a"))
	,(("ha2","carry"),("or1","b"))
	]

nodeTestHalfAdder = CompoundClass "test half adder" $ CompoundNodeClass 
	-- Inputs
	[("a",("halfadder1","a"))
	,("b",("halfadder1","b"))
	] 
	-- Outputs
	[]
	-- Names
	[("halfadder1","halfadder")
	,("sumprint","sumprinter")
	,("carryprint","carryprinter")
	]
	-- Arcs
	[(("halfadder1","sum"),("sumprint","in"))
	,(("halfadder1","carry"),("carryprint","in"))
	]

nodeTestFullAdder = CompoundClass "test full adder" $ CompoundNodeClass 
	-- Inputs
	[("a",("fa1","a"))
	,("b",("fa1","b"))
	,("c",("fa1","carry_in"))
	] 
	-- Outputs
	[]
	-- Names
	[("fa1","fulladder")
	,("sumprint","sumprinter")
	,("carryprint","carryprinter")
	]
	-- Arcs
	[(("fa1","sum"),("sumprint","in"))
	,(("fa1","carry_out"),("carryprint","in"))
	]


testHalfAdder = do
	maybe_root <- mkSystem [nodeTestHalfAdder,nodePrinter "sum: " "sumprinter",nodePrinter "carry: " "carryprinter",nodeHalfAdder,nodeAnd,nodeXor,nodeSpliter2] ("root","test half adder")
	case maybe_root of
		Nothing -> putStrLn "Did not compile"
		Just root -> do
			objectPush "a" (I 0) root
			objectPush "b" (I 1) root
			objectStart root
			return ()


testFullAdder = do
	maybe_root <- mkSystem 
		[nodeTestFullAdder
		,nodePrinter "sum: " "sumprinter"
		,nodePrinter "carry: " "carryprinter"
		,nodeHalfAdder
		,nodeAnd
		,nodeXor
		,nodeSpliter2
		,nodeFullAdder
		,nodeOr
		] 
		("root","test full adder")
	case maybe_root of
		Nothing -> putStrLn "Did not compile"
		Just root -> do
			objectPush "a" (I 1) root
			objectPush "b" (I 1) root
			objectPush "c" (I 0) root
			objectStart root
			return ()

