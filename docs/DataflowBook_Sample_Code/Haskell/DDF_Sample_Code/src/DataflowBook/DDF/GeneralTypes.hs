module DataflowBook.DDF.GeneralTypes where

type PortId = String
type ClassId = String
data Token 
	= I Int
	| S String
	| Pair Token Token
	deriving (Show,Eq)

type NodeId = String
type Address = (NodeId,PortId)

type LocalState = [(String,Int)] --TODO: change to Token values

