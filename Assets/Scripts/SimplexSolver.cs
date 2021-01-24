using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplexSolver
{
    double[,] tableau;
    double[] objective;
    double[,] original;
    int[] basic;
    int eqCount;
    int varCount;
    int totalVarCount;
    static readonly double e = 0.0001;
    bool inPhase1 = true;
    enum OUTCOME { NO_SOLUTION, UNBOUNDED, NOT_OPTIMIZED, OPTIMIZED };

    public SimplexSolver(float[,] lhs, float[] rhs, float[] objective)
    {
        this.eqCount = lhs.GetLength(0);
        this.varCount = lhs.GetLength(1);
        this.totalVarCount = eqCount + varCount;
        this.tableau = new double[eqCount, totalVarCount + 1];
        this.objective = new double[totalVarCount + 1];
        this.basic = new int[eqCount];
        this.original = new double[eqCount, varCount + 1];

        for (int i = 0; i < varCount; i++)
        {
            this.objective[i] = objective[i];
        }

        //Read in LHS of equations and add to tableau
        for (int i = 0; i < eqCount; i++)
        {
            for (int j = 0; j < varCount; j++)
            {
                this.tableau[i, j] = this.original[i, j] = lhs[i, j];
            }
        }

        //Read in RHS of equations and add to tableau
        for (int i = 0; i < eqCount; i++)
        {
            this.tableau[i, totalVarCount] = this.original[i, varCount] = rhs[i];
        }
    }

    public double[] Solve()
    {
        SetupTableau();

        OUTCOME result = Step();
        while (result == OUTCOME.NOT_OPTIMIZED)
        {
            //Debug.Log("steppign");
            result = Step();
        }
        if (result == OUTCOME.UNBOUNDED)
        {
            //Debug.Log("unbounded");
            return null;
        }
        else if (result == OUTCOME.NO_SOLUTION)
        {
            //Debug.Log("no solutio");
            return null;
        }
        else
        {
            //Debug.Log("optimized");
            double[] solution = new double[varCount];
            for (int i = 0; i < eqCount; i++)
            {
                if (basic[i] < varCount)
                {
                    solution[basic[i]] = tableau[i, totalVarCount] / tableau[i, basic[i]];
                }
            }
            //foreach (double f in solution) {
            //    Debug.Log(f);
            //}
            
            bool valid = ValidateSolution(solution);
            if (!valid) {
                Debug.LogWarning("invalid solution");
                return null;
            }
            return solution;
        }
    }

    void SetupTableau()
    {
        for (int i = 0; i < eqCount; i++)
        {
            //Add slack variable
            tableau[i, i + varCount] = 1;
            //Adjust for negative RHS
            if (tableau[i, totalVarCount] < -e)
            {
                for (int j = 0; j < totalVarCount + 1; j++)
                {
                    tableau[i, j] *= -1;
                }
            }
            basic[i] = i + varCount;
        }
    }


    bool ValidateSolution(double[] solution)
    {
        for (int i = 0; i < eqCount; i++)
        {
            double sum = 0;
            for (int j = 0; j < varCount; j++)
            {
                sum += solution[j] * original[i, j];
            }
            if (sum > original[i, varCount] + e)
            {
                string error = i + ", " + (sum-original[i,varCount]) + " :";
                for (int j = 0; j <= varCount; j++) {
                    error += original[i,j] + " ";
                }
                Debug.LogWarning(error);
                return false;
            }
        }
        return true;
    }

    //void readInput()
    //{
    //    Scanner scan = new Scanner(System.in);
    //    eqCount = scan.nextInt();
    //    varCount = scan.nextInt();
    //    totalVarCount = eqCount + varCount;
    //    tableau = new double[eqCount,totalVarCount + 1];
    //    objective = new double[totalVarCount + 1];
    //    basic = new int[eqCount];
    //    original = new double[eqCount,varCount + 1];

    //    //Read in LHS of equations and add to tableau
    //    for (int i = 0; i < eqCount; i++)
    //    {
    //        for (int j = 0; j < varCount; j++)
    //        {
    //            tableau[i,j] = original[i,j] = scan.nextInt();
    //        }
    //    }

    //    //Read in RHS of equations and add to tableau
    //    for (int i = 0; i < eqCount; i++)
    //    {
    //        tableau[i,totalVarCount] = original[i,varCount] = scan.nextInt();
    //    }

    //    //Read in objective function
    //    for (int i = 0; i < varCount; i++)
    //    {
    //        objective[i] = scan.nextInt();
    //    }
    //}



    OUTCOME Step()
    {
        //System.out.println("tableau:");
        //printArr(tableau);
        //System.out.println("objective:");
        //printArr(objective);



        //Find entering variable (variable with most positive coefficient in objective to maximize)
        int entering = 0;

        if (inPhase1)
        {
            //Debug.Log("phase 1");
            bool foundNegativeBasic = false;
            for (int i = 0; i < eqCount; i++)
            {
                if (tableau[i, basic[i]] < -e)
                {
                    foundNegativeBasic = true;
                    int maxCol = 0;
                    for (int j = 0; j < totalVarCount; j++)
                    {
                        if (tableau[i, j] > tableau[i, maxCol])
                        {
                            maxCol = j;
                        }
                    }
                    if (tableau[i, maxCol] < e)
                    {
                        return OUTCOME.NO_SOLUTION;
                    }
                    entering = maxCol;
                    break;
                }
            }
            if (!foundNegativeBasic)
            {
                //System.out.println("entering phase 2");
                inPhase1 = false;
            }
        }
        if (!inPhase1)
        {
            //Debug.Log("phase 2");
            for (int i = 1; i < totalVarCount; i++)
            {
                if (objective[i] > objective[entering])
                {
                    entering = i;
                }
            }
            if (objective[entering] < e)
            {
                return OUTCOME.OPTIMIZED;
            }
        }

        //Find row with minimum RHS/entering ratio (the equation that puts the tightest upper bound on the entering variable)
        int minRow = -1;
        double minRatio = double.MaxValue;
        for (int i = 0; i < eqCount; i++)
        {
            if (tableau[i, entering] > e && (tableau[i, totalVarCount] / tableau[i, entering] < minRatio - e || (inPhase1 && tableau[i, basic[i]] < -e && tableau[i, totalVarCount] / tableau[i, entering] < minRatio + e)))
            {
                minRatio = tableau[i, totalVarCount] / tableau[i, entering];
                minRow = i;
            }
        }
        //System.out.println("to row: "+minRow);
        //Unbounded
        if (minRow == -1)
        {
            return OUTCOME.UNBOUNDED;
        }

        //Divide row so that coefficient of entering variable = 1
        double ratio = 1 / tableau[minRow, entering];
        for (int i = 0; i < totalVarCount + 1; i++)
        {
            tableau[minRow, i] *= ratio;
        }

        //Set basic variable
        basic[minRow] = entering;

        //Clear entering variable from all other equations
        for (int i = 0; i < eqCount; i++)
        {
            if (i == minRow)
            {
                continue;
            }
            ratio = tableau[i, entering];
            for (int j = 0; j < totalVarCount + 1; j++)
            {
                tableau[i, j] -= tableau[minRow, j] * ratio;
            }
        }

        ratio = objective[entering];
        for (int i = 0; i < totalVarCount + 1; i++)
        {
            objective[i] -= tableau[minRow, i] * ratio;
        }

        return OUTCOME.NOT_OPTIMIZED;
    }

    //void printArr(double[,] arr)
    //{
    //    for (int i = 0; i < arr.length; i++)
    //    {
    //        for (int j = 0; j < arr[0].length; j++)
    //        {
    //            System.out.print(arr[i, j] + " ");
    //        }
    //        System.out.println();
    //    }
    //}

    //void printArr(double[] arr)
    //{
    //    for (int i = 0; i < arr.length; i++)
    //    {
    //        System.out.print(arr[i] + " ");
    //    }
    //    System.out.println();
    //}
}

