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

namespace Wikiled.Market.Analysis
{
    public class Classifier : IClassifier
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private SupportVectorMachine<Gaussian> model;

        public void Train(DataPackage data, CancellationToken token)
        {
            Guard.NotNull(() => data, data);
            log.Debug("Training with {0} records", data.Y.Length);

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

            // Search for the best model parameters
            var inputs = Accord.Statistics.Tools.Standardize(data.X);
            var result = gridsearch.Learn(inputs, data.Y);

            // Get the best SVM found during the parameter search
            SupportVectorMachine<Gaussian> svm = result.BestModel;

            // Instantiate the probabilistic calibration (using Platt's scaling)
            var calibration = new ProbabilisticOutputCalibration<Gaussian>(svm);
            
            // Run the calibration algorithm
            calibration.Learn(inputs, data.Y); // returns the same machine
            model = calibration.Model;
            var predicted = Classify(inputs);
            var confusionMatrix = new GeneralConfusionMatrix(classes: 2, expected: data.Y, predicted: predicted);
            log.Debug("Trained performance. F1(0):{0} F1(1):{1}", confusionMatrix.PerClassMatrices[0].FScore, confusionMatrix.PerClassMatrices[1].FScore);
        }

        public int[] Classify(double[][] x)
        {
            log.Debug("Classify");
            Guard.NotNull(() => x, x);
            return model.Decide(x).Select(item => item ? 1 : -1).ToArray();
        }
    }
}
