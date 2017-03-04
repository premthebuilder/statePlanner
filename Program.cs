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

        static void Main(string[] args)
        {
            Program pr = new Program();
            pr.RPC = new double[pr.sect];
            double[,] A = ACSVReader();
            DataTable dt = VectorReader();
            double[] RPC = VectorToRPC(dt);
            double[,] ARPC = CreateARPC(A,RPC);
            CreateLMatrix(ARPC);
            for (int i = 0; i < 382; i++)
            {
                for (int j = 0; j < 382; j++)
                {
                    if(i==j)
                    {
                        pr.IMatrix[i,j] = 1.0;
                    }
                    else
                    {
                        pr.IMatrix[i, j] = 0.0;
                    }
                }
            }
            for(int i=0;i<382;i++)
            {
                pr.RPC[i] = 0;                
            }

            
            for(int i=0; i<pr.sect; i++)//obtaining pA Matrix , multiplying 
            {
                for(int j=0;j<pr.sect; j++)
                {
                    pr.pAMatrix[j, i] = pr.IMatrix[j, i] * pr.RPC[j];
                }

            }
                
        }
    }
}
