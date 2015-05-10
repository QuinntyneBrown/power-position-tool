module DataflowBook.DDF.PipelineNodeObject where

import Control.Concurrent
import Control.Monad
import Data.List
import Data.Maybe
import DataflowBook.DDF.GeneralTypes
import DataflowBook.DDF.PipelineNodeClass

data PipelineNodeObject tIn tOut 
	= PipelineNodeObject 
		(Maybe (Chan tIn))   -- Input Chan
		(Maybe (Chan tOut))  -- Output Chan
		(Maybe ThreadId)     -- Action Thread
		((tIn,LocalState) -> IO ([tOut],LocalState)) -- Action Function

mkPipelineNode :: PipelineNodeClass tIn tOut -> PipelineNodeObject tIn tOut
mkPipelineNode cls = 
	PipelineNodeObject Nothing Nothing Nothing fn
	where
		PipelineNodeClass fn = cls

pipelineNodeConnectInput :: Chan tIn -> PipelineNodeObject tIn tOut -> IO (PipelineNodeObject tIn tOut)
pipelineNodeConnectInput chan_in obj = do
	PipelineNodeObject _ maybe_chan_out threadid fn <- pipelineNodeStop obj
	return (PipelineNodeObject (Just chan_in) maybe_chan_out threadid fn)

pipelineNodeConnectOutput :: Chan tOut -> PipelineNodeObject tIn tOut -> IO (PipelineNodeObject tIn tOut)
pipelineNodeConnectOutput chan_out obj = do
	PipelineNodeObject maybe_chan_in _ threadid fn <- pipelineNodeStop obj
	return (PipelineNodeObject maybe_chan_in (Just chan_out) threadid fn)

pipelineNodeStop :: PipelineNodeObject tIn tOut -> IO (PipelineNodeObject tIn tOut)
pipelineNodeStop obj = do
	let	PipelineNodeObject chan_in chan_out threadid fn = obj
	case threadid of
		Nothing -> return ()
		Just id -> killThread id
	return (PipelineNodeObject chan_in chan_out Nothing fn)

pipelineNodeStart :: PipelineNodeObject tIn tOut -> IO (PipelineNodeObject tIn tOut)
pipelineNodeStart obj = do
	PipelineNodeObject maybe_chan_in maybe_chan_out _ fn <- pipelineNodeStop obj
	case (maybe_chan_in,maybe_chan_out) of
		(Just chan_in, Just chan_out) -> do
			threadid <- forkIO $ pipelineNodeLooper [] chan_in chan_out fn
			return (PipelineNodeObject maybe_chan_in maybe_chan_out (Just threadid) fn)
		_ -> return $ PipelineNodeObject maybe_chan_in maybe_chan_out Nothing fn

pipelineNodeLooper localstate chan_in chan_out fn = do
	val_in <- readChan chan_in
	(vals_out,new_localstate) <- fn (val_in,localstate)
	mapM_ (writeChan chan_out) vals_out
	pipelineNodeLooper new_localstate chan_in chan_out fn
