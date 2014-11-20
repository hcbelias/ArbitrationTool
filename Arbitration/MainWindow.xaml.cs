using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Arbitration.Calculation;

namespace Arbitration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            this.TotalUnitsInput.Focus();
        }


        private void AddNewValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.AddNewValueInput.Text))
            {
                if (IsValidInput(this.AddNewValueInput.Text, false))
                {
                    this.ArbitrationGrid.Items.Add(new GridModel()
                    {
                        GoalNumberOfUnits = Double.Parse(this.AddNewValueInput.Text),
                        ArbitratedNumberOfUnits = 0
                    });
                    UpdateArbitration();
                    this.AddNewValueInput.Focus();
                    this.AddNewValueInput.SelectAll();
                }
                else
                {
                    ShowInvalidValueErrorMessage(this.AddNewValueInput.Text);
                }
            }
        }

        private bool IsValidInput(string input, bool isInt)
        {
            Match match = isInt ? Regex.Match(input, @"^[0-9]*$") : Regex.Match(input, @"^[0-9]*(?:\.[0-9]*)?$");

            if (match.Success)
            {
                bool parsedSuccess = false;
                double parsedValue = 0;
                parsedSuccess = Double.TryParse(input, out parsedValue);
                return parsedSuccess;
            }
            return false;
        }

        private void RemoveValue_Click(object sender, RoutedEventArgs e)
        {
            if (this.ArbitrationGrid.SelectedItems.Count > 0)
            {
                IEnumerator enumerator = this.ArbitrationGrid.SelectedItems.GetEnumerator();
                List<GridModel> itemsToBeRemoved = new List<GridModel>();
                while (enumerator.MoveNext())
                {
                    GridModel iterator = (GridModel)enumerator.Current;
                    itemsToBeRemoved.Add(iterator);
                }
                itemsToBeRemoved.ForEach(p =>
                {
                    this.ArbitrationGrid.Items.Remove(p);
                });
                UpdateArbitration();
            }
        }

        private void AddNewValueInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!IsValidInput(text, false))
                {
                    ShowInvalidValueErrorMessage(text.Substring(text.Count() - 1));
                    textBox.Text = text.Substring(0, text.Count() - 1);
                }
            }
        }

        private void ShowInvalidValueErrorMessage(string text)
        {
            ShowErrorMessage(string.Format("Invalid Value('{0}').", text));
        }



        private void ShowErrorMessage(string text)
        {
            MessageBox.Show(text);
        }

        private void UpdateArbitration()
        {
            if (this.TotalUnitsInput != null && !string.IsNullOrWhiteSpace(this.TotalUnitsInput.Text))
            {
                List<double> goalValues = GetGoalValues();
                if (goalValues.Any())
                {
                    int totalUnits = Int32.Parse(this.TotalUnitsInput.Text);
                    eArbitrationMethod method = (eArbitrationMethod)this.MethodCombobox.SelectedIndex;
                    Arbitration.Calculation.Arbitration calculator = new Arbitration.Calculation.Arbitration();
                    IList<int> arbitrateValues = calculator.Arbitrate(totalUnits, goalValues, method);
                    SetArbitrateValues(arbitrateValues);
                }
            }
        }

        private List<double> GetGoalValues()
        {
            return GetGridData().Select(p => p.GoalNumberOfUnits).ToList();
        }

        private List<GridModel> GetGridData()
        {
            List<GridModel> returnObj = new List<GridModel>();
            IEnumerable enumerable = this.ArbitrationGrid.Items.SourceCollection;
            IEnumerator enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                GridModel iterator = (GridModel)enumerator.Current;
                returnObj.Add(iterator);
            }
            return returnObj.ToList();
        }


        private void SetArbitrateValues(IList<int> arbitrateValues)
        {
            List<GridModel> returnObj = GetGridData();

            for (int i = 0; i < returnObj.Count; i++)
            {
                returnObj.ElementAt(i).ArbitratedNumberOfUnits = arbitrateValues[i];
            }

            this.ArbitrationGrid.Items.Refresh();
        }

        private void MethodCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateArbitration();
        }

        private void TotalUnitsInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!IsValidInput(text, true))
                {
                    ShowInvalidValueErrorMessage(text.Substring(text.Count() - 1));
                    textBox.Text = text.Substring(0, text.Count() - 1);
                }
                else
                {
                    UpdateArbitration();
                }
            }
        }

        private void PasteUnitsButton_Click(object sender, RoutedEventArgs e)
        {
            object contentCopy = Clipboard.GetData("Text");
            List<GridModel> oldGridData = this.GetGridData();
            if (contentCopy != null)
            {
                try
                {
                    string contentString = (string)contentCopy;

                    foreach (string row in contentString.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(row))
                        {
                            List<string> cellList = row.Split('\r', '\t').ToList();
                            cellList.RemoveAll(p => string.IsNullOrWhiteSpace(p));

                            switch (cellList.Count)
                            {
                                case 0:
                                    this.ArbitrationGrid.Items.Add(new GridModel()
                                    {
                                        CurrentNumberOfUnits = 0,
                                        GoalNumberOfUnits = 0,
                                        ArbitratedNumberOfUnits = 0
                                    });
                                    break;
                                case 1:
                                    this.ArbitrationGrid.Items.Add(new GridModel()
                                    {
                                        CurrentNumberOfUnits = 0,
                                        GoalNumberOfUnits = Double.Parse(cellList[0]),
                                        ArbitratedNumberOfUnits = 0
                                    });
                                    break;
                                default:
                                    this.ArbitrationGrid.Items.Add(new GridModel()
                                    {
                                        CurrentNumberOfUnits = Double.Parse(cellList[0]),
                                        GoalNumberOfUnits = Double.Parse(cellList[1]),
                                        ArbitratedNumberOfUnits = 0
                                    });
                                    break;
                            }
                        }
                    }
                    
                }
                catch (Exception error)
                {
                    this.ArbitrationGrid.Items.Clear();
                    oldGridData.ForEach(p => {
                        this.ArbitrationGrid.Items.Add(p);
                    });
                    ShowErrorMessage(error.Message);
                }
                finally
                {
                    UpdateArbitration();
                }
            }
        }


    }

    public class GridModel
    {
        public double GoalNumberOfUnits { get; set; }
        public double CurrentNumberOfUnits { get; set; }
        public int ArbitratedNumberOfUnits { get; set; }
    }


}
