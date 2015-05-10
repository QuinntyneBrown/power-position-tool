module DataflowBook.DDF.PrimitiveNodeObject where

import Control.Concurrent
import Control.Monad
import Data.List
import Data.Maybe
import DataflowBook.DDF.GeneralTypes
import DataflowBook.DDF.PipelineNodeObject
import DataflowBook.DDF.PipelineNodeClass
import DataflowBook.DDF.PrimitiveNodeClass

data PrimitiveNodeObject
	= PrimitiveNodeObject 
		[(PortId, PipelineNodeObject Token (PortId,Token))]      -- Inputs
		[(PortId, (Chan Token, PipelineNodeObject Token Token))] -- Outputs
		(Chan (PortId,Token))                                -- Input Chan
		(((PortId,Token),LocalState) -> IO ([(PortId,Token)],LocalState))  -- Action Fn
		(Maybe ThreadId)


mkPrimitiveNodeObject :: PrimitiveNodeClass -> IO PrimitiveNodeObject
mkPrimitiveNodeObject cls = do
	let (PrimitiveNodeClass in_portids out_portids action_fn) = cls
	in_chan <- newChan
	out_ports <- mapM mk_out_port out_portids
	in_ports <- mapM (mk_in_port in_chan) in_portids
	return $ PrimitiveNodeObject in_ports out_ports in_chan action_fn Nothing

	where
		mk_out_port :: PortId -> IO (PortId,(Chan Token, PipelineNodeObject Token Token))
		mk_out_port id = do
			let node = mkPipelineNode $ PipelineNodeClass (\(token,state) -> return ([token],state))
			chan <- newChan
			connected_node <- pipelineNodeConnectInput chan node
			return (id,(chan,connected_node))

		mk_in_port :: Chan (PortId,Token) -> PortId -> IO (PortId,PipelineNodeObject Token (PortId,Token))
		mk_in_port chan id = do
			let node = mkPipelineNode $ PipelineNodeClass (\(token,state) -> return ([(id,token)],state))
			connected_node <- pipelineNodeConnectOutput chan node
			return (id, connected_node)

primitiveNodeConnectInput :: Chan Token -> PortId -> PrimitiveNodeObject -> IO PrimitiveNodeObject
primitiveNodeConnectInput chan portid obj = do
	stopped_obj <- primitiveNodeStop obj
	let PrimitiveNodeObject in_ports out_ports in_chan action_fn _ = stopped_obj
	case (lookup portid in_ports) of
		Nothing -> return stopped_obj -- TODO: this should never happen
		Just node -> do
			new_node <- pipelineNodeConnectInput chan node
			let new_in_ports = (portid,new_node) : (filter (\(id,_) -> id /= portid) in_ports)
			return $ PrimitiveNodeObject new_in_ports out_ports in_chan action_fn Nothing

primitiveNodeConnectOutput :: Chan Token -> PortId -> PrimitiveNodeObject -> IO PrimitiveNodeObject
primitiveNodeConnectOutput chan portid obj = do
	stopped_obj <- primitiveNodeStop obj
	let PrimitiveNodeObject in_ports out_ports in_chan action_fn _ = stopped_obj
	case (lookup portid out_ports) of
		Nothing -> return stopped_obj -- TODO: this should never happen
		Just (port_chan, node) -> do
			new_node <- pipelineNodeConnectOutput chan node
			let new_out_ports = (portid, (port_chan,new_node)) : (filter (\(id,_) -> id /= portid) out_ports)
			return $ PrimitiveNodeObject in_ports new_out_ports in_chan action_fn Nothing

primitiveNodeStop :: PrimitiveNodeObject -> IO PrimitiveNodeObject
primitiveNodeStop obj = do
	let PrimitiveNodeObject in_ports out_ports in_chan action_fn maybe_threadid = obj
	new_in_ports <- mapM 
		(\(id,node) -> do
			new_node <- pipelineNodeStop node
			return (id,new_node)
		) in_ports
	new_out_ports <- mapM 
		(\(id,(chan,node)) -> do
			new_node <- pipelineNodeStop node
			return (id, (chan,new_node))
		) out_ports
	case maybe_threadid of
		Nothing -> return ()
		Just threadid -> killThread threadid
	return $ PrimitiveNodeObject new_in_ports new_out_ports in_chan action_fn Nothing

primitiveNodeStart :: PrimitiveNodeObject -> IO PrimitiveNodeObject
primitiveNodeStart obj = do
	PrimitiveNodeObject in_ports out_ports in_chan action_fn _ <- primitiveNodeStop obj
	new_in_ports <- mapM 
		(\(id,node) -> do
			new_node <- pipelineNodeStart node
			return (id,new_node)
		) in_ports
	new_out_ports <- mapM 
		(\(id,(chan,node)) -> do
			new_node <- pipelineNodeStart node
			return (id, (chan,new_node))
		) out_ports
	let out_chans = map (\(id,(chan,_)) -> (id,chan)) out_ports

	threadid <- case new_in_ports of
		[] -> forkIO $ primitiveSourceLooper out_chans [] action_fn
	 	_ -> forkIO $ primitiveLooper in_chan out_chans [] action_fn

	return $ PrimitiveNodeObject new_in_ports new_out_ports in_chan action_fn (Just threadid)

primitiveLooper :: Chan (PortId,Token) -> [(PortId,Chan Token)] -> LocalState -> (((PortId,Token),LocalState) -> IO ([(PortId,Token)],LocalState)) -> IO()
primitiveLooper in_chan out_chans state fn = do
	in_token <- readChan in_chan
	(tokens, new_state) <- fn (in_token,state)
	mapM_ 
		(\(id,val) -> do
			case (lookup id out_chans) of
				Nothing -> return ()
				Just chan -> writeChan chan val
		) tokens
	primitiveLooper in_chan out_chans new_state fn

nullPortId = ""
nullToken = I 0

primitiveSourceLooper :: [(PortId,Chan Token)] -> LocalState -> (((PortId,Token),LocalState) -> IO ([(PortId,Token)],LocalState)) -> IO()
primitiveSourceLooper out_chans state fn = do
	(tokens, new_state) <- fn ((nullPortId,nullToken),state)
	mapM_ 
		(\(id,val) -> do
			case (lookup id out_chans) of
				Nothing -> return ()
				Just chan -> writeChan chan val
		) tokens
	primitiveSourceLooper out_chans new_state fn

primitiveNodePush :: PortId -> Token -> PrimitiveNodeObject -> IO()
primitiveNodePush portid token obj = do
	let PrimitiveNodeObject in_ports out_ports in_chan action_fn _ = obj
	-- TODO: should verify that portid is valid
	writeChan in_chan (portid,token)
