using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

namespace GameOfLife
{
    public class GOLNode
    {
        public enum NodeAction
        {
            Waiting,    // For idle nodes and inbetween processing steps
            Die,        // Node will die
            StayAlive,  // Node will stay alive
            Populate    // Node will become alive
        }

        public static List<GOLNode> Map = new List<GOLNode>();

        public static int LBFDP = 1; // Requirement for de populization
        public static int UBFDP = 4; // Requirement for de populization
        public static int SBFL1 = 2; // Requirement for living 1
        public static int SBFL2 = 3; // Requirement for living 2
        public static int RQFBA = 3; // Requirement for becoming alive
        public static bool PathsFade = false;

        public Button Node { get; set; }

        public string Name { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public bool IsTaken { get; set; }
     
        private NodeAction NextAction;
        private int StepsWhileDeadCount = 0;

        private Brush DefaultBrush;

        #region Ctor

        /// <summary>
        /// Constructor saves button in variable, sets background to black, and is added to map
        /// </summary>
        /// <param name="node"></param>
        public GOLNode(Button node)
        {
            Node = node;
            DefaultBrush = Brushes.Black;
            Node.Background = Brushes.Black;
            Map.Add(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the background to limegreen and becomes taken
        /// </summary>
        public void TakeNode()
        {
            Node.Background = Brushes.LimeGreen;
            IsTaken = true;
        }
        /// <summary>
        /// Sets background to default brush and is no longer taken
        /// </summary>
        public void ResetNode()
        {
            NextAction = NodeAction.Waiting;
            Node.Background = DefaultBrush;
            IsTaken = false;
        }
        /// <summary>
        /// Gets the nieghbor count of this node
        /// </summary>
        /// <returns></returns>
        public int NeighborCount()
        {
            int count = 0;
            GOLNode tl = GOLNode.Map.FirstOrDefault(n => n.X == X - 1 && n.Y == Y + 1);
            GOLNode tt = GOLNode.Map.FirstOrDefault(n => n.X == X - 0 && n.Y == Y + 1);
            GOLNode tr = GOLNode.Map.FirstOrDefault(n => n.X == X + 1 && n.Y == Y + 1);
            GOLNode rr = GOLNode.Map.FirstOrDefault(n => n.X == X + 1 && n.Y == Y + 0);
            GOLNode br = GOLNode.Map.FirstOrDefault(n => n.X == X + 1 && n.Y == Y - 1);
            GOLNode bb = GOLNode.Map.FirstOrDefault(n => n.X == X + 0 && n.Y == Y - 1);
            GOLNode bl = GOLNode.Map.FirstOrDefault(n => n.X == X - 1 && n.Y == Y - 1);
            GOLNode ll = GOLNode.Map.FirstOrDefault(n => n.X == X - 1 && n.Y == Y - 0);
            if (tl != null && tl.IsTaken) count++;
            if (tt != null && tt.IsTaken) count++;
            if (tr != null && tr.IsTaken) count++;
            if (rr != null && rr.IsTaken) count++;
            if (br != null && br.IsTaken) count++;
            if (bb != null && bb.IsTaken) count++;
            if (bl != null && bl.IsTaken) count++;
            if (ll != null && ll.IsTaken) count++;
            return count;
        }
        /// <summary>
        /// Sets the action of the node for the next processing step
        /// </summary>
        /// <param name="action"></param>
        public void SetNextAction(NodeAction action)
        {
            NextAction = action;
        }
        /// <summary>
        /// Does the set NodeAction
        /// </summary>
        public void DoNextAction()
        {
            switch (NextAction)
            {
                case NodeAction.Die:
                    {
                        KillNode();
                        SetNextAction(NodeAction.Waiting);
                        StepsWhileDeadCount = 0;
                        break;
                    }
                case NodeAction.Populate:
                    {
                        TakeNode();
                        break;
                    }
                case NodeAction.StayAlive:
                    {
                        SetNextAction(NodeAction.Waiting);
                        break;
                    }
            }
            if (PathsFade && (IsTaken == false && Node.Background == Brushes.PaleVioletRed))
            {
                StepsWhileDeadCount += 1;
                if (StepsWhileDeadCount > 8)
                {
                    Node.Background = Brushes.Black;
                    StepsWhileDeadCount = 0;
                }
            }
        }
        /// <summary>
        /// Game Of Life Ruling Function 
        /// This function can change this node
        /// </summary>
        public void NodeRuling()
        {
            if (IsTaken)
            {
                int nnc = NeighborCount();
                if (nnc >= UBFDP || nnc <= LBFDP)
                {
                    SetNextAction(NodeAction.Die);
                }
                if (nnc == SBFL1 || nnc == SBFL2)
                {
                    SetNextAction(NodeAction.StayAlive);
                }
            }
            else
            {
                if (NeighborCount() == RQFBA)
                {
                    SetNextAction(NodeAction.Populate);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the background to red and is no longer taken
        /// </summary>
        private void KillNode()
        {
            Node.Background = Brushes.PaleVioletRed;
            IsTaken = false;
        }
        /// <summary>
        /// Sets all the nodes to a waiting state for the next processing step
        /// </summary>
        private static void SetForNextTurn()
        {
            GOLNode.Map.ForEach(node => node.DoNextAction());
            GOLNode.Map.ForEach(node => node.SetNextAction(GOLNode.NodeAction.Waiting));
            GOLNode.Map.ForEach(node => node.DoNextAction());
        }

        #endregion 
    }
}
