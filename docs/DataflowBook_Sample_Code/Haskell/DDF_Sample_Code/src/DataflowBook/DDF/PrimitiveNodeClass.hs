module DataflowBook.DDF.PrimitiveNodeClass where

import DataflowBook.DDF.GeneralTypes

data PrimitiveNodeClass
	= PrimitiveNodeClass 
		[PortId]  -- Inputs
		[PortId]  -- Outputs
		(((PortId,Token),LocalState) -> IO ([(PortId,Token)],LocalState)) -- Action Fn
