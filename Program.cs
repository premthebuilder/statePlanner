using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Data;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ptest1
{
    class Program
    {
        int sect = 382;
        double[,] IMatrix = new double[382,382];
        double[] RPC;
        double[,] pAMatrix;

        /// <summary>
        /// A Matrix from CSV File
        /// </summary>
        static double[,] ACSVReader()
        {
            string filePath = @"C:\Users\Prashanth D\Desktop\New folder\UIchanges\PEIaPP\PEIaPP\Dump\NJ_AMatrix.csv";
            TextReader reader = File.OpenText(filePath);
            
            StreamReader sr = new StreamReader(filePath);
            var lines = new List<string[]>();
            int Row = 0;
            while (!sr.EndOfStream)
            {
                string[] Line = sr.ReadLine().Split(',');
                lines.Add(Line);
                Row++;
                
            }

            var data = lines.ToArray();

            int len = data.Length;
            double[,] doubleData = new double[len,len];
            for(int i=0;i<len; i++ )
            {
                for(int j=0;j<len; j++)
                {
                    doubleData[i, j] = double.Parse(data[i][j])   ;
                }
            }
            return doubleData;
        }
        /// <summary>
        /// Retreives Vector Data from CSV file 
        /// </summary>
        /// <returns></returns>
        static DataTable VectorReader()
        {
            string filePath = @"C:\Users\Prashanth D\Desktop\New folder\UIchanges\PEIaPP\PEIaPP\Dump\34.csv";
            //TextReader reader = File.OpenText(filePath);
            DataTable dt = new DataTable();

            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }

            return dt;
        }
        /// <summary>
        /// Datatable to RPC Array Extraction
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        /// 
        static double[] VectorToRPC(DataTable dt)
        {
            double[] RPC = new double[dt.Rows.Count];

            int i = 0;
            foreach (DataRow row in dt.Rows)
            {                
                foreach(DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == "rpc")
                    RPC[i] = double.Parse((row[dt.Columns.IndexOf(col)]).ToString());                    
                }
                i++;
            }
            //RPC = 
            return RPC;
        }

        /// <summary>
        /// Creating L Matrix
        /// </summary>
        /// <param name="ARPC"></param>
        /// <returns></returns>
        static double[,] CreateLMatrix(double[,] ARPC)
        {
            Matrix<double> Imat = DenseMatrix.CreateIdentity(382);
            Matrix<double> ARPCmat = DenseMatrix.OfArray(ARPC);
            return (Imat - ARPCmat).Inverse().ToArray();
        }

        /// <summary>
        /// Multiply A Matrix with RPC column
        /// </summary>
        /// <param name="A"></param>
        /// <param name="RPC"></param>
        /// <returns></returns>
        /// 
        static double[,] CreateARPC(double[,] A, double[] RPC)
        {
            //Matrix<double> Amat = DenseMatrix.OfArray(A);
            //Matrix<double> RPCmat = DenseMatrix.OfColumnMajor(1, RPC.Length, RPC);
            //Matrix<double> ARPC = 
            //Amat.Multiply(RPCmat, ARPC);

            double[,] ARPC = new double[382, 382];

            for (int i = 0; i < 382; i++)//obtaining pA Matrix , multiplying 
            {
                for (int j = 0; j < 382; j++)
                {
                    ARPC[j, i] = A[j, i] * RPC[j];
                }

            }
            return ARPC;
        }

        /// <summary>
        /// Calculate the Output Disturbance(Col H Direct Effects)
        /// </summary>
        /// <param name="sectors"></param>
        /// <param name="earnings"></param>
        /// <param name="output"></param>
        /// <param name="dist"></param>
        /// <param name="jobsBy1000">Vectors - Col-N</param>
        /// <param name="earningsByOutput">Vectors - Col -O</param>
        /// <returns></returns>
        static double[] CreateOutpuDisturbance(int sectors, double earnings, double output, double [] dist, double[] jobsBy1000, double[] earningsByOutput)
        {
            double[] OPDist = new double[sectors];

            if (output == 0)
            {
                if (earnings == 0)
                {
                    for (int i = 0; i < sectors; i++)
                    {
                        OPDist[i] = dist[i] / jobsBy1000[i];
                    }
                }
                else
                {
                    for (int i = 0; i < sectors; i++)
                    {
                        OPDist[i] = dist[i] / earningsByOutput[i];
                    }
                }
            }
            else
            {
                OPDist = dist;
            }

            return OPDist;
        }

        /// <summary>
        /// Creating Direct Effects Columns:Col - J-M Direct Effects
        /// </summary>
        /// <param name="sectors"></param>
        /// <param name="RPC"></param>
        /// <param name="OPDist"></param>
        /// <param name="jobsBy1000">Vectors - Col-N</param>
        /// <param name="earningsByOutput">Vectors - Col -O</param>
        /// <param name="valueAddedByOutput">Vectors - Col - P</param>
        /// <param name="Output">DirectEffects-Col j</param>
        /// <param name="EmpPerOut">DirectEffects-Col K</param>
        /// <param name="EarPerOut">DirectEffects-Col L</param>
        /// <param name="ValAddPerOut">DirectEffects-Col M</param>
        /// <returns></returns>
        static bool CreateDirectEffects(int sectors, double[] RPC, double[]OPDist, double[] jobsBy1000,double[] earningsByOutput, double[] valueAddedByOutput, out double[] Output,out double[] EmpPerOut, out double[] EarPerOut, out double[] ValAddPerOut)
        {
            bool result = true;
            Output = new double[sectors];
            EmpPerOut = new double[sectors];
            EarPerOut = new double[sectors];
            ValAddPerOut = new double[sectors];

            //output
            //to be clarified as it the same as not using RPC
            Output = OPDist;

            //Employement Per Output
            for (int i = 0; i < sectors; i++)
            {
                EmpPerOut[i] = Output[i] * jobsBy1000[i];
            }

            //Earning PEr Output
            for (int i = 0; i < sectors; i++)
            {
                EarPerOut[i] = Output[i] * earningsByOutput[i];
            }

            //Value Added Per Output
            for (int i = 0; i < sectors; i++)
            {
                ValAddPerOut[i] = Output[i] * valueAddedByOutput[i];
            }

            return result;
        }

        /// <summary>
        /// Creating Total Effects Columns:Col - Q-W Direct Effects
        /// </summary>
        /// <param name="sectors"></param>
        /// <param name="L"></param>
        /// <param name="OPDist"></param>
        /// <param name="jobsBy1000">Vectors - Col-N</param>
        /// <param name="earningsByOutput">Vectors - Col -O</param>
        /// <param name="valueAddedByOutput">Vectors - Col - P</param>
        /// <param name="FedTaxOp">>Vectors - Col - S</param>
        /// <param name="StTaxOp">>Vectors - Col - T</param>
        /// <param name="LclTaxOp">>Vectors - Col - U</param>
        /// <param name="Output">DirectEffects-Col Q</param>
        /// <param name="EmpPerOut">DirectEffects-Col R</param>
        /// <param name="EarPerOut">DirectEffects-Col S</param>
        /// <param name="ValAddPerOut">DirectEffects-Col T</param>
        /// <param name="FedGenPerOut">DirectEffects-Col U</param>
        /// <param name="StPerOut">DirectEffects-Col V</param>
        /// <param name="LclPerOut">DirectEffects-Col W</param>
        /// <returns></returns>
        static bool CreateTotalEffects(int sectors, Matrix L, double[] OPDist, double[] jobsBy1000, double[] earningsByOutput, double[] valueAddedByOutput, double[] FedTaxOp, double[] StTaxOp, double[] LclTaxOp, out double[] Output, out double[] EmpPerOut, out double[] EarPerOut, out double[] ValAddPerOut, out double[] FedGenPerOut,out double[] StPerOut, out double[] LclPerOut)
        {
            bool result = true;
            Output = new double[sectors];
            EmpPerOut = new double[sectors];
            EarPerOut = new double[sectors];
            ValAddPerOut = new double[sectors];
            FedGenPerOut = new double[sectors];
            StPerOut = new double[sectors];
            LclPerOut = new double[sectors];

            // Creating multidimentional double array from array, to form matrix later
            double[,] OPDistMult = new double[1, OPDist.Length];
            for (int i = 0; i < OPDist.Length; ++i)
                OPDistMult[0, i] = OPDist[i];
            Matrix<double> OPDistMat = DenseMatrix.OfArray(OPDistMult);
            var op = L.Multiply(OPDistMat);
            //MAtrix to single Dimension Array
            for (int i = 0; i < sectors; i++)
            {
                Output[i] = op[i, 0];
            }

            //Employement Per Output
            for (int i = 0; i < sectors; i++)
            {
                EmpPerOut[i] = Output[i] * jobsBy1000[i];
            }

            //Earning PEr Output
            for (int i = 0; i < sectors; i++)
            {
                EarPerOut[i] = Output[i] * earningsByOutput[i];
            }

            //Value Added Per Output
            for (int i = 0; i < sectors; i++)
            {
                ValAddPerOut[i] = Output[i] * valueAddedByOutput[i];
            }

            //Federal General Per Output
            for (int i = 0; i < sectors; i++)
            {
                FedGenPerOut[i] = Output[i] * FedTaxOp[i];
            }

            //State PEr Output
            for (int i = 0; i < sectors; i++)
            {
                StPerOut[i] = Output[i] * StTaxOp[i];
            }

            //Local Per Output
            for (int i = 0; i < sectors; i++)
            {
                LclPerOut[i] = Output[i] * LclTaxOp[i];
            }

            return result;
        }


        static bool CreateDetailedIndustryTotEff()
        {

        }

        static void Main(string[] args)
        {
            Program pr = new Program();
            pr.RPC = new double[pr.sect];
            double[,] A = ACSVReader();
            DataTable dt = VectorReader();
            double[] RPC = VectorToRPC(dt);
            double[,] ARPC = CreateARPC(A,RPC);


            CreateLMatrix(ARPC);
            
            
            
                
        }
    }
}
