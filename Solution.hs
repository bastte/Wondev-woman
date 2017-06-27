import System.IO
import System.Random
import Control.Monad
import Data.List

type Unit = (Int, Int)
type Grid = [[Square]]
type Square = Maybe Int
type Action = (String, Int, String, String)
data Game = Game { units :: [Unit], others :: [Unit], grid :: Grid, actions :: [Action] }

main :: IO ()
main = do
    hSetBuffering stdout NoBuffering
    
    size <- readLn
    unitsPerPlayer <- readLn
    loop size unitsPerPlayer
    
getUnit :: IO Unit 
getUnit = (\[x, y] -> (x,y)) . map read . words <$> getLine
 
getSquare :: IO [Square]
getSquare = map (\x -> if x == "." then Nothing else Just $ read x) . words <$> getLine 

getAction :: IO Action
getAction = (\[atype, index, dir1, dir2] -> (atype, read index, dir1, dir2)) . words <$> getLine

getRandom :: [a] -> IO a
getRandom x = return . (x!!) =<< randomRIO (0, length x-1)

step :: Game -> IO Action
step = getRandom . actions

stringify :: Action -> String
stringify (atype, index, dir1, dir2) = concat $ intersperse " " $ [atype, show index, dir1, dir2] 

loop :: Int -> Int -> IO ()
loop size unitsperplayer = do
    grid <- replicateM size getSquare
        
    units <- replicateM unitsperplayer getUnit
    
    others <- replicateM unitsperplayer getUnit
    legalActionsCount <- readLn
    legalActions <- replicateM legalActionsCount getAction
    let game = Game units others grid legalActions
    putStrLn =<< stringify <$> step game
    
    loop size unitsperplayer
    
