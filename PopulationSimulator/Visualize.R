d=read.csv("/Users/ndelaney/Dropbox/EvolutionExperimentDB/InferenceMachinary/PopulationSimulator/bin/Release/Results.Log")
d=read.delim("c:/Users/Nigel/Documents/Dropbox/EvolutionExperimentDB/InferenceMachinary/PopulationSimulator/bin/Release/Results.Log")
plot(d$Rate)
plot(d$TotalSimulations)
q=grep("Bin",colnames(d))

mdbfe=apply(d[,q],2,mean)
plot(mdbfe)
