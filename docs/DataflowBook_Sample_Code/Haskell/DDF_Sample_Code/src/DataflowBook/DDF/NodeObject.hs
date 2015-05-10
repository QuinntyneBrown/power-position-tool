module DataflowBook.DDF.NodeObject where

import Control.Concurrent
import Control.Monad
import Data.List
import Data.Maybe

import DataflowBook.DDF.GeneralTypes
import DataflowBook.DDF.PrimitiveNodeObject
import DataflowBook.DDF.NodeClass

data CompoundNodeObject
	= CompoundNodeObject 
		[(PortId,Address)] -- Input Port Mappings
		[(PortId,Address)] -- Output Port Mappings
		[Object] -- Locals Objects


mkCompoundNode :: [Class] -> CompoundNodeClass -> IO (Maybe CompoundNodeObject)
mkCompoundNode classes root = do

	maybe_child_objs <- mapM mk_local_object object_decl

	case (all isJust maybe_child_objs) of
		False -> 
			error "ERROR (mkCompoundNode): Some classes did not build"
			--return Nothing
		True -> do
			let child_objs = map fromJust maybe_child_objs

			connected_objs <- foldM connectOne child_objs arc_decl

			return $ Just $ CompoundNodeObject in_ports out_ports connected_objs

	where
		CompoundNodeClass in_ports out_ports object_decl arc_decl = root

		mk_local_object :: (NodeId,ClassId) -> IO (Maybe Object)
		mk_local_object (node_id,class_id) = mkSystem classes (node_id,class_id) 

		connectOne :: [Object] -> (Address,Address) -> IO [Object]
		connectOne objs (source_address,sink_address) = do
			let maybe_source_obj = get_obj source_address objs
			let maybe_sink_obj = get_obj sink_address objs

			case (maybe_source_obj,maybe_sink_obj) of
				(Just source_obj, Just sink_obj) -> do
					chan <- newChan
					let (source_node_id, source_port_id) = source_address
					let (sink_node_id, sink_port_id) = sink_address
					new_source_obj <- objectConnectOutput chan source_port_id source_obj
					new_sink_obj <- objectConnectInput chan sink_port_id sink_obj

					let new_objs = new_source_obj : (new_sink_obj : (filter (\o -> source_node_id /= objectGetNodeId o) (filter (\o -> sink_node_id /= objectGetNodeId o) objs))) -- remove old objs and add new
					return new_objs
				_ -> 
					error (concat ["ERROR (mkCompoundNode): source: ", show source_address, " or sink: ", show sink_address, " not found"])

			where
				-- Return Nothing if obj is not found 
				get_obj :: Address -> [Object] -> Maybe Object
				get_obj (nodeid,portid) objs = 
					find (\o -> nodeid == objectGetNodeId o) objs

compoundNodeConnectInput :: Chan Token -> PortId -> CompoundNodeObject -> IO CompoundNodeObject
compoundNodeConnectInput chan portid node = do
	let CompoundNodeObject in_ports out_ports objs = node

	case (lookup portid in_ports) of
		Nothing -> error $ concat ["ERROR (compoundNodeConnectInput): output port not found: ", show portid] 
		Just (address_node, address_port) -> do
			case (find (\o -> address_node == objectGetNodeId o) objs) of
				Nothing -> 
					error $ concat ["ERROR (compoundNodeConnectInput): address given for port not found: ", show address_node]
				Just term_node -> do
					new_term_node <- objectConnectInput chan address_port term_node
					let new_objs = new_term_node : (filter (\o -> address_node /= objectGetNodeId o) objs)
					return $ CompoundNodeObject in_ports out_ports new_objs

compoundNodeConnectOutput :: Chan Token -> PortId -> CompoundNodeObject -> IO CompoundNodeObject
compoundNodeConnectOutput chan portid node = do
	let CompoundNodeObject in_ports out_ports objs = node

	case (lookup portid out_ports) of
		Nothing -> error $ concat ["ERROR (compoundNodeConnectOutput): output port not found: ", show portid] 
		Just (address_node, address_port) -> do
			case (find (\o -> address_node == objectGetNodeId o) objs) of
				Nothing -> 
					error $ concat ["ERROR (compoundNodeConnectOutput): address given for port not found: ", show address_node]
				Just term_node -> do
					new_term_node <- objectConnectOutput chan address_port term_node
					let new_objs = new_term_node : (filter (\o -> address_node /= objectGetNodeId o) objs)
					return $ CompoundNodeObject in_ports out_ports new_objs

compoundNodeStart :: CompoundNodeObject -> IO CompoundNodeObject
compoundNodeStart node = do
	let CompoundNodeObject in_ports out_ports objs = node
	new_objs <- mapM objectStart objs
	return $ CompoundNodeObject in_ports out_ports new_objs

compoundNodeStop :: CompoundNodeObject -> IO CompoundNodeObject
compoundNodeStop node = do
	let CompoundNodeObject in_ports out_ports objs = node
	new_objs <- mapM objectStop objs
	return $ CompoundNodeObject in_ports out_ports new_objs

compoundNodePush :: PortId -> Token -> CompoundNodeObject -> IO()
compoundNodePush portid token obj = do
	let CompoundNodeObject in_ports out_ports objs = obj
	case (lookup portid in_ports) of
		Nothing -> do
			putStrLn $ show in_ports
			error $ concat ["ERROR (compoundNodePush): port not found: ", show portid]
		Just (address_nodeid,address_portid) -> do
			case (find (\o -> address_nodeid == objectGetNodeId o) objs) of
				Nothing -> error "ERROR (compoundNodePush): node not found (should never happen)"
				Just node -> objectPush address_portid token node

data Object
	= CompoundObj NodeId CompoundNodeObject
	| PrimitiveObj NodeId PrimitiveNodeObject


objectGetNodeId :: Object -> NodeId
objectGetNodeId obj = 
	case obj of
		CompoundObj id _ -> id
		PrimitiveObj id _ -> id

mkSystem :: [Class] -> (NodeId,ClassId) -> IO (Maybe Object)
mkSystem classes (node_id,class_id) = do
	case (find (\c -> class_id == classGetClassId c) classes) of
		Nothing -> 
			error (concat ["ERROR (mkSystem): class not found: ", show class_id])
		Just cls -> 
			case cls of
				CompoundClass _ c -> do
					maybe_node <- mkCompoundNode classes c
					case maybe_node of
						Nothing -> return Nothing
						Just obj -> return $ Just $ CompoundObj node_id obj
				PrimitiveClass _ c -> do
					node <- mkPrimitiveNodeObject c
					return $ Just $ PrimitiveObj node_id node


objectConnectInput :: Chan Token -> PortId -> Object -> IO Object
objectConnectInput chan port_id obj = do
	case obj of
		CompoundObj id node -> do
			new_node <- compoundNodeConnectInput chan port_id node
			return $ CompoundObj id new_node
		PrimitiveObj id node -> do
			new_node <- primitiveNodeConnectInput chan port_id node
			return $ PrimitiveObj id new_node

objectConnectOutput :: Chan Token -> PortId -> Object -> IO Object
objectConnectOutput chan port_id obj = do
	case obj of
		CompoundObj id node -> do
			new_node <- compoundNodeConnectOutput chan port_id node
			return $ CompoundObj id new_node
		PrimitiveObj id node -> do
			new_node <- primitiveNodeConnectOutput chan port_id node
			return $ PrimitiveObj id new_node

objectStart :: Object -> IO Object
objectStart obj = do
	case obj of
		CompoundObj id node -> do
			new_node <- compoundNodeStart node
			return $ CompoundObj id new_node
		PrimitiveObj id node -> do
			new_node <- primitiveNodeStart node
			return $ PrimitiveObj id new_node

objectStop :: Object -> IO Object
objectStop obj = do
	case obj of
		CompoundObj id node -> do
			new_node <- compoundNodeStop node
			return $ CompoundObj id new_node
		PrimitiveObj id node -> do 
			new_node <- primitiveNodeStop node
			return $ PrimitiveObj id new_node

objectPush :: PortId -> Token -> Object -> IO()
objectPush portid token obj = do
	case obj of
		PrimitiveObj _ node -> primitiveNodePush portid token node
		CompoundObj _ node -> compoundNodePush portid token node

