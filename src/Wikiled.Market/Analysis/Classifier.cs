using System;
using System.Linq;
using System.Threading;
using Accord.MachineLearning;
using Accord.MachineLearning.Performance;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;
using Accord.Statistics.Kernels;
using NLog;
using Wikiled.Common.Arguments;
using Wikiled.MachineLearning.Mathematics;
using Wikiled.MachineLearning.Normalization;

namespace Wikiled.Market.Analysis
{
    public class Classifier : IClassifier
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private SupportVectorMachine<Gaussian> model;

        private Standardizer standardizer;

        public GeneralConfusionMatrix TestSetPerformance { get; private set;}

        public void Train(DataPackage data, CancellationToken token)
        {
            Guard.NotNull(() => data, data);
            log.Debug("Training with {0} records", data.Y.Length);

            standardizer = Standardizer.GetNumericStandardizer(data.X);
            var xTraining = data.X;
            var yTraining = data.Y;

            var xTesting = xTraining;
            var yTesting = yTraining;

            int testSize = 100;
            
            if (xTraining.Length > testSize * 4)
            {
                var training = xTraining.Length - testSize;
                xTesting = xTraining.Skip(training).ToArray();
                yTesting = yTraining.Skip(training).ToArray();
                xTraining = xTraining.Take(training).ToArray();
                yTraining = yTraining.Take(training).ToArray();
            }

            xTraining = standardizer.StandardizeAll(xTraining);
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

            gridsearch.Token = token;

            var randomized = new Random().Shuffle(xTraining, yTraining).ToArray();
            yTraining = randomized[1].Cast<int>().ToArray();
            xTraining = randomized[0].Cast<double[]>().ToArray();

            var result = gridsearch.Learn(xTraining, yTraining);

            // Get the best SVM found during the parameter search
            SupportVectorMachine<Gaussian> svm = result.BestModel;

            // Instantiate the probabilistic calibration (using Platt's scaling)
            var calibration = new ProbabilisticOutputCalibration<Gaussian>(svm);
            
            // Run the calibration algorithm
            calibration.Learn(xTraining, yTraining); // returns the same machine
            model = calibration.Model;
            var predicted = ClassifyInternal(xTraining);
            var confusionMatrix = new GeneralConfusionMatrix(classes: 2, expected: yTraining, predicted: predicted);
            log.Debug("Performance on training dataset . F1(0):{0} F1(1):{1}", confusionMatrix.PerClassMatrices[0].FScore, confusionMatrix.PerClassMatrices[1].FScore);

            predicted = Classify(xTesting);
            confusionMatrix = new GeneralConfusionMatrix(classes: 2, expected: yTesting, predicted: predicted);
            TestSetPerformance = confusionMatrix;
            log.Debug("Performance on testing dataset . F1(0):{0} F1(1):{1}", confusionMatrix.PerClassMatrices[0].FScore, confusionMatrix.PerClassMatrices[1].FScore);
        }

        public int[] Classify(double[][] x)
        {
            log.Debug("Classify");
            Guard.NotNull(() => x, x);
            x = standardizer.StandardizeAll(x);
            return ClassifyInternal(x);
        }

        private int[] ClassifyInternal(double[][] x)
        {
            return model.Decide(x).Select(item => item ? 1 : -1).ToArray();
        }
    }
}
