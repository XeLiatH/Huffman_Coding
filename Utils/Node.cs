using System;

namespace Huffman
{
    public class Node
    {
        public char Character
        {
            private set;
            get;
        }

        public int Frequency
        {
            private set;
            get;
        }
        public Node Left
        {
            private set;
            get;
        }

        public Node Right
        {
            private set;
            get;
        }

        public Node(char character, int frequency, Node left = null, Node right = null)
        {
            this.Character = character;
            this.Frequency = frequency;
            this.Left = left;
            this.Right = right;
        }

        public bool IsLeaf()
        {
            return this.Left == null && this.Right == null;
        }

        // public static bool operator >=(Node a, Node b)
        // {
        //     return a.Frequency >= b.Frequency;
        // }

        // public static bool operator <=(Node a, Node b)
        // {
        //     return a.Frequency <= b.Frequency;
        // }

        // public static bool operator >(Node a, Node b)
        // {
        //     return a.Frequency > b.Frequency;
        // }

        // public static bool operator <(Node a, Node b)
        // {
        //     return a.Frequency < b.Frequency;
        // }

        // public static bool operator ==(Node a, Node b)
        // {
        //     return a.Frequency == b.Frequency;
        // }

        // public static bool operator !=(Node a, Node b)
        // {
        //     return a.Frequency != b.Frequency;
        // }
    }
}