using System;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.ML.Train;
using Encog.ML.Data.Basic;
using Encog;
using MicrosoftExcel = Microsoft.Office.Interop;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Encog.App.Analyst;
using Encog.App.Analyst.Wizard;
using Encog.App.Analyst.Report;
using Encog.Util.File;
using Encog.App.Analyst.CSV;
using Encog.Util.CSV;

using Encog.App.Analyst.CSV.Normalize;
using Encog.Util.Arrayutil;
using Encog.Util.Simple;

namespace TestCompIn
{
    internal class Program
    {

        public static double learningRate;  // to be provided by user 

        public static double UMomentum;  // To be provided by user

        public static int epoch ;  // to be provided by user

  

        private static void Main(string[] args)
        {

           

          
                Console.WriteLine("Press 1 for selecting  Regresssion and 2 for classification");
                int whatToperform = int.Parse(Console.ReadLine());


                Console.WriteLine("Please provide number of layers assuming first layer is input layer and last is output layer");
                int numberOfLayers = int.Parse(Console.ReadLine());



                var network = new BasicNetwork();

                for (int i = 1; i <= numberOfLayers; i++)
                {
                    Console.WriteLine("Please select the activation function for layer- {0}", i);  // Activtion function Input
                    Console.WriteLine("Press 1 for ActivationBiPolar ");
                    Console.WriteLine("Press 2 for ActivationCompetitive  ");
                    Console.WriteLine("Press 3 for ActivationLinear ");
                    Console.WriteLine("Press 4 for ActivationLog  ");
                    Console.WriteLine("Press 5 for ActivationSigmoid  ");
                    Console.WriteLine("Press 6 for ActivationSoftMax ");
                    Console.WriteLine("Press 7 for ActivationTanh  ");
                    Console.WriteLine("Press 8 for default  ");
                    int whichActivation = int.Parse(Console.ReadLine());


                    Console.WriteLine("Please the bias for this layer : 1 for True and 0 for false ");   // Bias input
                    int whichBias = int.Parse(Console.ReadLine());



                    Console.WriteLine("Please the enter the neuron count for this layer");   // Neuron count input
                    int countNeuron = int.Parse(Console.ReadLine());


                    switch (whichActivation)   // building the network
                    {

                        case 1: network.AddLayer(new BasicLayer(new ActivationBiPolar(), Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        case 2: network.AddLayer(new BasicLayer(new ActivationCompetitive(), Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        case 3: network.AddLayer(new BasicLayer(new ActivationLinear(), Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        case 4: network.AddLayer(new BasicLayer(new ActivationLOG(), Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        case 5: network.AddLayer(new BasicLayer(new ActivationSigmoid(), Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        case 6: network.AddLayer(new BasicLayer(new ActivationSoftMax(), Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        case 7: network.AddLayer(new BasicLayer(new ActivationTANH(), Convert.ToBoolean(whichBias), countNeuron));
                            break;
                        case 8: network.AddLayer(new BasicLayer(null, Convert.ToBoolean(whichBias), countNeuron));
                            break;

                        default:
                            Console.WriteLine("Wrong data entered - Application will stop   ");
                            break;
                    }

                }

                network.Structure.FinalizeStructure();  //complete the newtork settings
                network.Reset();

                Console.WriteLine("Please enter the learning rate ");   // learning rate input
                learningRate = double.Parse(Console.ReadLine());

                Console.WriteLine("Please enter the momentum value");   // Momentum input
                UMomentum = double.Parse(Console.ReadLine());

                Console.WriteLine("Please the enter the number of epochs ");   // epoch input
                epoch = int.Parse(Console.ReadLine());


            // For Regression we do this piece of code

            if (whatToperform == 1)
            {


                var sourceFile = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\regression_train.csv");  //fetch training file
                var targetFile = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\Result\khicharNormClassificationTrainData.csv");  //save train normalized file


                var sourceFileTest = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\regression_train.csv"); //fetch testing file
                var targetFileTest = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\Result\khicharNormClassificationTestData.csv"); //Save test normalized file

                //Analyst
                var analyst = new EncogAnalyst();
                //Wizard
                var wizard = new AnalystWizard(analyst);
                wizard.TargetFieldName = "y";  //set the output variable  for regression . it is not necessary when using mutliple attributes
                wizard.Wizard(sourceFile, true, AnalystFileFormat.DecpntComma);

                //norm for Training
                var norm = new AnalystNormalizeCSV();
                norm.Analyze(sourceFile, true, CSVFormat.English, analyst);
                norm.ProduceOutputHeaders = true;
                norm.Normalize(targetFile);

                //norm for testing

                norm.Analyze(sourceFileTest, true, CSVFormat.English, analyst);
                norm.Normalize(targetFileTest);


                analyst.Save(new FileInfo("stt.ega"));

                


                var trainingset1 = EncogUtility.LoadCSV2Memory(targetFile.ToString(), network.InputCount, network.OutputCount, true, CSVFormat.English, false);


                var train = new Backpropagation(network, trainingset1);
                int epo = 1;
                do
                {
                    train.Iteration();
                    Console.WriteLine(@"Epoch #" + epo + @" Error:" + train.Error);
                    epo++;

                    if (epo > epoch)
                    {
                        break;
                    }

                } while (train.Error > 0.05);


                var evaluationSet = EncogUtility.LoadCSV2Memory(targetFileTest.ToString(), network.InputCount, network.OutputCount, true, CSVFormat.English, false);

               
                

                List<Tuple<double, double>> inputExcel = new List<Tuple<double, double>>();

                foreach (var item in evaluationSet)
                {
                    
                    var output = network.Compute(item.Input);
                  
                    inputExcel.Add(new Tuple<double, double>(item.Input[0], output[0]));
                }


                PlotRegressionTest(inputExcel);
                 
                Console.WriteLine("----------------Execution over - check the Regression output excel ------------------------------------");
                Console.ReadKey();
                EncogFramework.Instance.Shutdown();

            }

            //End of Regression


        //     For classification we do this piece of code

                if (whatToperform == 2)
                {

                    // fetch train file 
                    var sourceFile = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\data.circles.test.1000.csv");
                    var targetFile = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\result\khicharNormClassificationTrainData.csv");


                    ///fetch test file
                    var sourceFileTest = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\data.circles.test.1000.csv");
                    var targetFileTest = new FileInfo(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\result\khicharNormClassificationTestData.csv");

                    //Analyst
                    var analyst = new EncogAnalyst();
                    //Wizard
                    var wizard = new AnalystWizard(analyst);
                    wizard.Wizard(sourceFile, true, AnalystFileFormat.DecpntComma);

                    //norm for Training
                    var norm = new AnalystNormalizeCSV();
                    norm.Analyze(sourceFile, true, CSVFormat.English, analyst);
                    norm.ProduceOutputHeaders = true;
                    norm.Normalize(targetFile);

                    //norm for testing

                    norm.Analyze(sourceFileTest, true, CSVFormat.English, analyst);
                    norm.Normalize(targetFileTest);


                    analyst.Save(new FileInfo("stt.ega"));

                   
                    var trainingset1 = EncogUtility.LoadCSV2Memory(targetFile.ToString(), network.InputCount, network.OutputCount, true, CSVFormat.English, false);


                    var train = new Backpropagation(network, trainingset1);
                    int epo = 1;
                    do
                    {
                        train.Iteration();
                        Console.WriteLine(@"Epoch #" + epo + @" Error:" + train.Error);
                        epo++;

                        if (epo > epoch)
                        {
                            break;
                        }

                    } while (train.Error > 0.05);


                    var evaluationSet = EncogUtility.LoadCSV2Memory(targetFileTest.ToString(), network.InputCount, network.OutputCount, true, CSVFormat.English, false);

                    int count = 0;
                    int CorrectCount = 0;

                    List<Tuple<double, double, double>> inputExcel = new List<Tuple<double, double, double>>();

                    foreach (var item in evaluationSet)
                    {
                        count++;
                        var output = network.Compute(item.Input);
                        // int classCount = analyst.Script.Normalize.NormalizedFields[4].Classes.Count;

                        int classCount = analyst.Script.Normalize.NormalizedFields[2].Classes.Count;
                        double normalizationHigh = analyst.Script.Normalize.NormalizedFields[2].NormalizedHigh;
                        double normalizationLow = analyst.Script.Normalize.NormalizedFields[2].NormalizedLow;

                        var eq = new Encog.MathUtil.Equilateral(classCount, normalizationHigh, normalizationLow);
                        var predictedClassInt = eq.Decode(output);
                        var predictedClass = analyst.Script.Normalize.NormalizedFields[2].Classes[predictedClassInt].Name;
                        var idealClassInt = eq.Decode(item.Ideal);
                        var idealClass = analyst.Script.Normalize.NormalizedFields[2].Classes[idealClassInt].Name;

                        if (predictedClassInt == idealClassInt)
                        {
                            CorrectCount++;
                        }


                        inputExcel.Add(new Tuple<double, double, double>(item.Input[0], item.Input[1], Convert.ToDouble(predictedClass)));
                    }



                    Console.WriteLine("Total Test Count : {0}", count);
                    Console.WriteLine("Total Correct Prediction Count : {0}", CorrectCount);
                    Console.WriteLine("% Success : {0}", ((CorrectCount * 100.0) / count));
                    PlotClassificationTest(inputExcel);

                    Console.WriteLine("----------------Execution over - check the Classification output excel ------------------------------------");
                    Console.ReadKey();
                    EncogFramework.Instance.Shutdown();



                  }  //End of classification

        }





        // Plotting for Regression

        public static void PlotRegressionTest(List<Tuple<double , double>> inputExcel)
        {

            Microsoft.Office.Interop.Excel.Application oXL = null;
            Microsoft.Office.Interop.Excel._Workbook oWB = null;
            Microsoft.Office.Interop.Excel._Worksheet oSheet = null;

            try
            {
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oWB = oXL.Workbooks.Open(@"C:\Users\smandia\Desktop\Attachments_20161015\Result\Book3");
                oSheet = String.IsNullOrEmpty("Sheet1") ? (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet : (Microsoft.Office.Interop.Excel._Worksheet)oWB.Worksheets["Sheet1"];

                oSheet.Cells[1, 1] = "X";
                oSheet.Cells[1, 2] = " Predicated Y";
               


                int length = inputExcel.Count;



                int i = 2;  // start putting values from here 
                 
                    foreach (Tuple<double , double  > t in inputExcel)
                    {
                       
                           oSheet.Cells[i, 1] = t.Item1;
                           oSheet.Cells[i, 2] = t.Item2;
                           

                           i++;
                    }
              

                    oWB.SaveAs(@"C:\Users\smandia\Desktop\Attachments_20161015\Result\shilpaRegression");

              //  MessageBox.Show("Done!");
            }
            catch (Exception ex)
            {
              //  MessageBox.Show(ex.ToString());
            }
            finally
            {
               

                GC.Collect();
                GC.WaitForPendingFinalizers();
               
                Marshal.ReleaseComObject(oSheet);

                //close and release
                oWB.Close();
                Marshal.ReleaseComObject(oWB);

                //quit and release
                oXL.Quit();
                Marshal.ReleaseComObject(oXL);
            }
        }


        public static void PlotClassificationTest(List<Tuple<double, double, double>> inputExcel)
        {

            Microsoft.Office.Interop.Excel.Application oXL = null;
            Microsoft.Office.Interop.Excel._Workbook oWB = null;
            Microsoft.Office.Interop.Excel._Worksheet oSheet = null;

            try
            {
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oWB = oXL.Workbooks.Open(@"C:\Users\smandia\Desktop\Attachments_20161015\Result\Book3");
                oSheet = String.IsNullOrEmpty("Sheet1") ? (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet : (Microsoft.Office.Interop.Excel._Worksheet)oWB.Worksheets["Sheet1"];

                oSheet.Cells[1, 1] = "X";
                oSheet.Cells[1, 2] = "Y";
                oSheet.Cells[1, 3] = " Category";


                int length = inputExcel.Count;



                int i = 2;  // start putting values from here 

                foreach (Tuple<double, double, double> t in inputExcel)
                {

                    oSheet.Cells[i, 1] = t.Item1;
                    oSheet.Cells[i, 2] = t.Item2;
                    oSheet.Cells[i, 3] = t.Item3;
                   

                    i++;
                }

                oWB.SaveAs(@"C:\Users\smandia\Desktop\Attachments_20161015\Attachments_20161029\Result\shilpaClassification");

                Console.WriteLine("Finised with the classification program - please check the excel file"); ;
            }
            catch (Exception ex)
            {
                 
                  Console.WriteLine(ex.ToString());
            }
            finally
            {


                GC.Collect();
                GC.WaitForPendingFinalizers();
                Marshal.ReleaseComObject(oSheet);

                //close and release
                oWB.Close();
                Marshal.ReleaseComObject(oWB);

                //quit and release
                oXL.Quit();
                Marshal.ReleaseComObject(oXL);
            }
        }


    }
}