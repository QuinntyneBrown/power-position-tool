module DataflowBook.DDF.NodeClass where

import DataflowBook.DDF.GeneralTypes
import DataflowBook.DDF.PrimitiveNodeClass

data CompoundNodeClass
	= CompoundNodeClass 
		[(PortId,Address)] -- Input Port to actual address
		[(PortId,Address)] -- Output Port to actual address
		[(NodeId,ClassId)] -- Local name definitions
		[(Address,Address)] -- Arc definitions

data Class 
	= CompoundClass ClassId CompoundNodeClass
	| PrimitiveClass ClassId PrimitiveNodeClass

classGetClassId :: Class -> ClassId
classGetClassId cls = 
	case cls of
		CompoundClass id _ -> id
		PrimitiveClass id _ -> id
