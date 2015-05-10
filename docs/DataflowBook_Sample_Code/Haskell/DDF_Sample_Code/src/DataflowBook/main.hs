module Main where

import DataflowBook.DDF.Test
import Control.Concurrent

main = do
	testFullAdder
	threadDelay 1000000000 -- wait so we can see output
