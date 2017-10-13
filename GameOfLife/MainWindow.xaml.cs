using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

namespace GameOfLife
{
    public partial class MainWindow : Window
    {
        private List<GOLNode> Nodes = new List<GOLNode>();

        private string InputErrorTitle = Properties.Resources.AliveNodeLabelText;
        private string InputErrorMessage = Properties.Resources.InputErrorMessage;
        private string AliveNodeLabelText = Properties.Resources.AliveNodeLabelText;
        private string LargestPopLabelText = Properties.Resources.LargestPopLabelText;
        private string TotalStepsLabelText = Properties.Resources.TotalStepsLabelText;

        private Action EmptyAction = new Action(() => { });

        #region Constructor 

        /// <summary>
        /// Dispatcher Invoke here to know when all child components have been loaded
        /// and to be able to call NodeSetup
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => NodesSetup()));
        }
        /// <summary>
        /// Sets up each node on the map
        /// </summary>
        private void NodesSetup()
        {
            int rM = 52;
            int r = 0;
            int cM = 51;
            int c = 0;
            foreach (Button node in FindVisualChildren<Button>(this).Where(b => String.IsNullOrEmpty(b.Name)))
            {
                node.Name = string.Format("N_{0}_{1}", r, c);
                node.Click += Node_Click;
                node.MouseEnter += Node_Hover;
                Nodes.Add(new GOLNode(node)
                {
                    Name = node.Name,
                    X = c,
                    Y = r
                });
                if (++c > 51)
                {
                    c = 0;
                    ++r;
                }
            }
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Starts Game Of Life
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            bool lbfdp = Int32.TryParse(LRFDPTextBox.Text, out GOLNode.LBFDP);
            bool ubfdp = Int32.TryParse(UPFDPTextBox.Text, out GOLNode.UBFDP);
            bool sbfl1 = Int32.TryParse(RTSA1TextBox.Text, out GOLNode.SBFL1);
            bool sbfl2 = Int32.TryParse(RTSA2TextBox.Text, out GOLNode.SBFL2);
            bool rqfba = Int32.TryParse(RTPOPTextBox.Text, out GOLNode.RQFBA);
            int steps = 0;
            if (lbfdp && ubfdp && sbfl1 && sbfl2 && rqfba)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, dwea) => GOLNode.Map.ForEach(node => node.NodeRuling());
                for (;;)
                {
                    worker.RunWorkerAsync();
                    while (worker.IsBusy)
                    {
                        Dispatcher.Invoke(DispatcherPriority.ContextIdle, EmptyAction);
                    }
                    steps += 1;
                    GOLNode.Map.ForEach(node => node.DoNextAction());
                    AliveNodesLabel.Content = string.Format(AliveNodeLabelText, GOLNode.Map.Count(n => n.IsTaken));
                    LargestPopulationLabel.Content = string.Format(LargestPopLabelText, GOLNode.Map.Max(n => n.NeighborCount()));
                    TotalStepsLabel.Content = string.Format(TotalStepsLabelText, steps);
                    GOLNode.Map.ForEach(node => node.SetNextAction(GOLNode.NodeAction.Waiting));
                    GOLNode.Map.ForEach(node => node.DoNextAction());
                }
            }
            else
            {
                MessageBox.Show(InputErrorMessage, InputErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Sets a node to a certain state depending on its current state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Node_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GOLNode gnode = GOLNode.Map.Single(node => node.Name == button.Name);
            if (!gnode.IsTaken)
            {
                gnode.TakeNode();
            }
            else
            {
                gnode.ResetNode();
            }
        }
        /// <summary>
        /// Does Node Click but only if the user holds down the left mouse button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Node_Hover(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Button button = sender as Button;
                GOLNode gnode = GOLNode.Map.Single(n => n.Name == button.Name);
                if (!gnode.IsTaken)
                {
                    gnode.TakeNode();
                }
                else
                {
                    gnode.ResetNode();
                }
            }
        }
        
        #endregion

        #region Static

        /// <summary>
        /// Helpers function to find components of a parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        #endregion
    }
}
