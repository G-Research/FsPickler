﻿#r "bin/Debug/FsPickler.dll"

open FsPickler
open System
open System.IO
open System.Reflection

let fsc = new FsPickler()

let p = fsc.ResolvePickler<int * string option []> ()

let loop (x : 'T) =
    use mem = new MemoryStream()
    fsc.Serialize<obj>(mem, x)
    mem.Position <- 0L
    fsc.Deserialize<obj>(mem) :?> 'T


type Foo = A | B of int

type Peano = Zero | Succ of Peano

let pp = 
    Pickler.fix(fun peanoP -> 
        peanoP 
        |> Pickler.option 
        |> Pickler.wrap 
            (function None -> Zero | Some p -> Succ p) 
            (function Zero -> None | Succ p -> Some p))

let rec int2Peano n = match n with 0 -> Zero | n -> Succ(int2Peano (n-1))

let p = int2Peano 42

p |> fsc.Pickle pp |> fsc.UnPickle pp


type Tree<'T> = Empty | Node of 'T * Forest<'T>
and Forest<'T> = Nil | Cons of Tree<'T> * Forest<'T>


let rec nTree (n : int) =
    if n = 0 then Empty
    else
        Node(n, nForest (n-1) (n-1))

and nForest (d : int) (l : int) =
    if l = 0 then Nil
    else
        Cons(nTree d, nForest d (l-1))


let get<'T> () =
    Pickler.fix2(fun treeP forestP ->
        let tp = Pickler.auto<'T>

        let treeP' =
            Pickler.pair tp forestP
            |> Pickler.option 
            |> Pickler.wrap 
                (function None -> Empty | Some (v,f) -> Node(v,f))
                (function Empty -> None | Node (v,f) -> Some(v,f))

        let forestP' =
            Pickler.pair treeP forestP
            |> Pickler.option
            |> Pickler.wrap
                (function None -> Nil | Some (t,f) -> Cons(t,f))
                (function Nil -> None | Cons(t,f) -> Some (t,f))

        treeP', forestP')


let tp, fp = get<int> ()


let n = nTree 5

n |> fsc.Pickle tp |> fsc.UnPickle tp