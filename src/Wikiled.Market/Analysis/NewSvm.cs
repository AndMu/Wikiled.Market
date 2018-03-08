using System;
using System.Linq;
using Accord.MachineLearning;
using Accord.MachineLearning.Performance;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using Wikiled.MachineLearning.Svm.Logic;

namespace Wikiled.Market.Analysis
{
    public class NewSvm
    {
        public void Classify(Problem problem)
        {
            // Ensure results are reproducible
            Accord.Math.Random.Generator.Seed = 0;

            // Instantiate a new Grid Search algorithm for Kernel Support Vector Machines
            var gridsearch = new GridSearch<SupportVectorMachine<Gaussian>, double[], int>()
            {
                // Here we can specify the range of the parameters to be included in the search
                ParameterRanges = new GridSearchRangeCollection
                                                           {
                                                               new GridSearchRange("complexity", new [] { 0.001, 0.01, 0.1, 1, 10 }),
                                                               new GridSearchRange("gamma",     new [] { 0.001, 0.01, 0.1, 1 })
                                                           },

                // Indicate how learning algorithms for the models should be created
                Learner = p => new SequentialMinimalOptimization<Gaussian>
                {
                    Complexity = p["complexity"],
                    Kernel = new Gaussian
                    {
                        Gamma = p["gamma"]
                    }
                },

                // Define how the performance of the models should be measured
                Loss = (actual, expected, m) => new ZeroOneLoss(expected).Loss(actual)
            };

            // If needed, control the degree of CPU parallelization
            gridsearch.ParallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount / 2;

            // Search for the best model parameters
            var inputs = problem.X.Select(item => item.Select(x => x.Value).ToArray()).ToArray();
            var result = gridsearch.Learn(inputs, problem.Y);

            // Get the best SVM found during the parameter search
            SupportVectorMachine<Gaussian> svm = result.BestModel;

            var predicted = svm.Decide(inputs).Select(item => item ? 1 : 0).ToArray();
            var cm = new GeneralConfusionMatrix(classes: 2, expected: problem.Y, predicted: predicted);
            var fScoreNegative = cm.PerClassMatrices[0].FScore;
            var fScorePostive = cm.PerClassMatrices[1].FScore;

            // Instantiate the probabilistic calibration (using Platt's scaling)
            var calibration = new ProbabilisticOutputCalibration<Gaussian>(svm);
            
            // Run the calibration algorithm
            calibration.Learn(inputs, problem.Y); // returns the same machine

            predicted = calibration.Model.Decide(inputs).Select(item => item ? 1 : 0).ToArray();
            var cm2 = new GeneralConfusionMatrix(classes: 2, expected: problem.Y, predicted: predicted);
            var fScoreNegativeScaled = cm2.PerClassMatrices[0].FScore;
            var fScorePostiveScaled = cm2.PerClassMatrices[1].FScore;
        }
    }
}
