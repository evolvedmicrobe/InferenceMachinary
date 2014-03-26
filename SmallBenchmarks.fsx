#time 
#r @"C:\Users\Nigel\Documents\Dropbox\EvolutionExperimentDB\mathnet-numerics\out\lib\Net40\MathNet.Numerics.dll"
open MathNet.Numerics.Random
open MathNet.Numerics.Distributions

let rn=new MersenneTwister(false);

let v=[0 .. 10000]
v |> Seq.map (fun (z:int)->rn.NextDouble()) |> Seq.sum

let makeR (n:int) =
    let mutable tot=0
    let mutable nl=n
    while nl>0 do
        tot<-tot+Poisson.Sample(rn,600.0)
        nl <- nl-1
    tot    
    
makeR 10000000
    
    