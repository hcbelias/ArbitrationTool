using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbitration.Calculation
{
    public enum eArbitrationMethod
    {
        DistanceToGoal = 0,
        PercentageToGoal = 1
    }

    public class Arbitration
    {
        #region Public Methods

        /// <summary>
        /// Creates an array of integers, splitting a number of units among a number of options,
        /// based on a "Goal"
        /// </summary>
        /// <param name="totalNumberOfUnits">Total number of units to split</param>
        /// <param name="goalValues">The Goals for each item</param>
        /// <param name="arbitrationMethod">The type of rounding to use</param>
        /// <param name="initialValues">
        /// Use this if you already have a starting point and want to arbitrate more units over an
        /// existing collection.
        /// </param>
        /// <param name="limitsOfUnits">
        /// Limits individually the maximum unit for each element. If the value reaches the limit i
        /// won't get any more unis, even if has a higher difference to goal.
        /// </param>
        /// <returns>
        /// List with the values distributed as close to the Goal as possible, using the given
        /// rounding method
        /// </returns>
        public IList<int> Arbitrate(int totalNumberOfUnits, ICollection<double> goalValues, eArbitrationMethod arbitrationMethod,
            IList<int> initialValues = null, IList<int> limitsOfUnits = null)
        {
            return (arbitrationMethod == eArbitrationMethod.DistanceToGoal)
                ? DistanceToGoal(totalNumberOfUnits, goalValues, initialValues, limitsOfUnits)
                : PercentageToGoal(totalNumberOfUnits, goalValues, initialValues, limitsOfUnits);
        }

        /// <summary>
        /// Creates an array of integers, splitting a number of units among a number of options,
        /// based on a "Goal" using the method "Distance To Goal"
        /// </summary>
        /// <param name="totalNumberOfUnits">Total number of units to split</param>
        /// <param name="goalValues">The Goals for each item</param>
        /// <param name="initialValues">
        /// Use this if you already have a starting point and want to arbitrate more units over an
        /// existing collection.
        /// </param>
        /// <param name="limitsOfUnits">
        /// Limits individually the maximum unit for each element. If the value reaches the limit i
        /// won't get any more unis, even if has a higher difference to goal.
        /// </param>
        /// <returns>
        /// List with the values distributed as close to the Goal as possible, using the given
        /// method "Distance To Goal"
        /// </returns>
        public IList<int> DistanceToGoal(int totalNumberOfUnits, ICollection<double> goalValues, IList<int> initialValues = null,
            IList<int> limitsOfUnits = null)
        {
            IList<ArbitrationCalc> calcList = CreateStandardList(goalValues, round => round.Goal - round.Value, initialValues);
            return CalculateArbitration(totalNumberOfUnits, calcList, limitsOfUnits);
        }

        /// <summary>
        /// Creates an array of integers, splitting a number of units among a number of options,
        /// based on a "Goal" using the method "Percentage to Goal"
        /// </summary>
        /// <param name="totalNumberOfUnits">Total number of units to split</param>
        /// <param name="goalValues">The Goals for each item</param>
        /// <param name="initialValues">
        /// Use this if you already have a starting point and want to arbitrate more units over an
        /// existing collection.
        /// </param>
        /// <param name="limitsOfUnits">
        /// Limits individually the maximum unit for each element. If the value reaches the limit i
        /// won't get any more unis, even if has a higher difference to goal.
        /// </param>
        /// <returns>
        /// List with the values distributed as close to the Goal as possible, using the given
        /// method "Percentage to Goal"
        /// </returns>
        public IList<int> PercentageToGoal(int totalNumberOfUnits, ICollection<double> goalValues, IList<int> initialValues = null,
            IList<int> limitsOfUnits = null)
        {
            IList<ArbitrationCalc> calcList = CreateStandardList(goalValues, round => 1 - (round.Value / round.Goal), initialValues);
            return CalculateArbitration(totalNumberOfUnits, calcList, limitsOfUnits);
        }

        #endregion Public Methods

        #region Private Methods

        private IList<int> CalculateArbitration(int totalNumberOfUnits, IList<ArbitrationCalc> calcList, IList<int> LimitsOfUnits = null)
        {
            //If the total number of units is higher than the sum of the limits of units, throws an exception, since it won't be possible to
            //split the total among the possibilities available
            if ((LimitsOfUnits != null) && (totalNumberOfUnits > LimitsOfUnits.Sum(s => s)))
                throw new ArgumentOutOfRangeException("totalNumberOfUnits");

            //Adds units to each value in the collection based on the highest difference to the Goal.
            //The Difference is calculated based on the rounding method provided
            //If it was set the limits of units, only increase quantity if the limits is not reached yet, no matter the difference
            for (int aux = 0; aux < totalNumberOfUnits; aux++)
            {
                ArbitrationCalc item = calcList
                    .Select((v, i) => new { Item = v, Index = i })
                    .Where(v => (LimitsOfUnits == null) || (v.Item.Value < LimitsOfUnits[v.Index]))
                    .OrderByDescending(r => r.Item.Difference)
                    .ThenByDescending(r => r.Item.Goal)
                    .Select(v => v.Item)
                    .FirstOrDefault();

                item.Value = item.Value + 1;
            }

            return calcList.Select(c => c.Value).ToList();
        }

        private IList<ArbitrationCalc> CreateStandardList(ICollection<double> goalValues, Func<ArbitrationCalc, double> calculationMethod,
            IList<int> initialValues = null)
        {
            return goalValues.Select((d, i) =>
                new ArbitrationCalc() { Goal = d, CalculationMethod = calculationMethod, Value = (initialValues == null) ? 0 : initialValues[i] }).ToList();
        }

        #endregion Private Methods

    }

    public class ArbitrationCalc
    {
        #region Public Properties

        public Func<ArbitrationCalc, double> CalculationMethod { get; set; }

        public double Difference { get { return CalculationMethod(this); } }

        public double Goal { get; set; }

        public int Value { get; set; }

        #endregion Public Properties
    }
}