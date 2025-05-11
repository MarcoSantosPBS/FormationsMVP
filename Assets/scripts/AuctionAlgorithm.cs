using System;
using System.Collections.Generic;

public class AuctionAlgorithm
{
    private readonly double[,] costMatrix;
    private readonly int n;
    private readonly double epsilon;
    private readonly double[] prices;
    private readonly int[] assignments;
    private readonly int[] reverseAssignments;

    public AuctionAlgorithm(double[,] costMatrix, double epsilon = 1e-3)
    {
        this.costMatrix = costMatrix;
        this.n = costMatrix.GetLength(0);
        this.epsilon = epsilon;
        this.prices = new double[n];
        this.assignments = new int[n];
        this.reverseAssignments = new int[n];

        for (int i = 0; i < n; i++)
        {
            assignments[i] = -1;
            reverseAssignments[i] = -1;
        }
    }

    public int[] Solve()
    {
        var unassignedAgents = new Queue<int>();
        for (int i = 0; i < n; i++)
        {
            unassignedAgents.Enqueue(i);
        }

        while (unassignedAgents.Count > 0)
        {
            int agent = unassignedAgents.Dequeue();
            double maxProfit = double.NegativeInfinity;
            double secondMaxProfit = double.NegativeInfinity;
            int bestTask = -1;

            for (int task = 0; task < n; task++)
            {
                double profit = -costMatrix[agent, task] - prices[task];
                if (profit > maxProfit)
                {
                    secondMaxProfit = maxProfit;
                    maxProfit = profit;
                    bestTask = task;
                }
                else if (profit > secondMaxProfit)
                {
                    secondMaxProfit = profit;
                }
            }

            double bid = maxProfit - secondMaxProfit + epsilon;
            prices[bestTask] += bid;

            if (reverseAssignments[bestTask] != -1)
            {
                int previousAgent = reverseAssignments[bestTask];
                assignments[previousAgent] = -1;
                unassignedAgents.Enqueue(previousAgent);
            }

            assignments[agent] = bestTask;
            reverseAssignments[bestTask] = agent;
        }

        return assignments;
    }
}