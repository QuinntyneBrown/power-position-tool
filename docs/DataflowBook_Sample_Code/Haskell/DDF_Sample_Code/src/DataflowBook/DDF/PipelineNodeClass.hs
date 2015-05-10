module DataflowBook.DDF.PipelineNodeClass where

import DataflowBook.DDF.GeneralTypes

data PipelineNodeClass tIn tOut 
	= PipelineNodeClass 
		((tIn,LocalState) -> IO ([tOut],LocalState)) -- Action Fn
