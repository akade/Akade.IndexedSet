// Ignore Spelling: Ukkonen

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Akade.IndexedSet.DataStructures;

/// <summary>
/// Based on https://github.com/baratgabor/SuffixTree (MIT)
/// Adapted to be poolable, modernized and optimized
/// </summary>
internal class UkkonenTree
{
    public const int BOUNDLESS = -1;

    private int _position = -1;
    private int _remainder = 0;
    private int _activeLength = 0;

    internal readonly Node _root = new() { Start = 0, End = 0 };
    private Node _activeParent;
    private Node? _activeEdge;
    private Node? _needSuffixLink;

    internal readonly List<char> _chars = [];

    private readonly List<Node> _nodePool = [];
    private int _nodesInUse = 0;

    public UkkonenTree()
    {
        _activeParent = _root;
        Clear();
    }

    public void Clear()
    {
        _position = -1;
        _remainder = 0;
        _activeLength = 0;

        _activeParent = _root;
        _activeEdge = null;
        _needSuffixLink = null;

        _root.Children.Clear();
        _chars.Clear();

        _nodesInUse = 0;
    }

    private void ResetEdge()
    {
        _activeLength = 0;
        _activeEdge = null;
    }

    private bool MoveDown(char c)
    {
        if (_activeEdge == null &&
            !GetEdgeFor(_activeParent, c, out _activeEdge))
        {
            return false; // Cannot lock on edge
        }
        else if (_chars[_activeEdge.Start + _activeLength] != c)
        {
            return false; // Cannot match next char on edge
        }

        _activeLength++; // Success matching. Simply locking on a new edge is a match too, since it equals a first char match.

        if (!_activeEdge.IsLeaf && _activeLength == LengthOf(_activeEdge))
        {
            _activeParent = _activeEdge;
            ResetEdge();
        }

        return true;
    }

    private void Rescan()
    {
        // If we can't jump to linked node, we need to jump to root, and use Remainder as ActiveLength
        if (!GetLinkFor(_activeParent, out _activeParent!))
        {
            _activeEdge = null;
            _activeParent = _root;
            _activeLength = _remainder - 1;
        }

        if (_activeLength == 0)
        {
            return;
        }

        // Keep jumping through edges until we find the first edge that is shorter than our remaining length
        while (GetEdgeFor(_activeParent, _chars[_position - _activeLength], out _activeEdge)
                && _activeLength >= LengthOf(_activeEdge))
        {
            _activeLength -= LengthOf(_activeEdge);
            _activeParent = _activeEdge;
        }
    }

    /// <summary>
    /// Extends the suffix tree with the specified value.
    /// </summary>
    public void Add(ReadOnlySpan<char> value)
    {
        // TODO: What about terminating with unique character?
        _chars.EnsureCapacity(_chars.Count + value.Length);
        foreach (char c in value)
        {
            ExtendTree(c);
        }

        _remainder = 0;
        ResetEdge();
        _activeParent = _root;
    }

    private void ExtendTree(char c)
    {
        _chars.Add(c);
        _needSuffixLink = null;
        _position++;
        _remainder++;

        while (_remainder > 0)
        {
            if (MoveDown(c))
            {
                break;
            }

            if (_activeEdge != null)
            {
                _activeParent = InsertSplit();
            }

            InsertLeaf(c);
            _remainder--;

            if (_remainder > 0)
            {
                Rescan();
            }
        }
    }

    private int LengthOf(Node edge)
    {
        return (edge.End == -1 ? _position + 1 : edge.End) - edge.Start;
    }

    private char FirstCharOf(Node edge)
    {
        return _chars[edge.Start];
    }

    private bool GetLinkFor(Node node, [NotNullWhen(true)] out Node? linkedNode)
    {
        linkedNode = node.SuffixLink;
        return linkedNode is not null;
    }

    private bool GetEdgeFor(Node n, char c, [NotNullWhen(true)] out Node? edge)
    {
        return n.Children.TryGetValue(c, out edge);
    }

    private Node InsertLeaf(char c)
    {
        Node node = CreateNode(_position, BOUNDLESS);
        _activeParent.Children.Add(c, node);

        return node;
    }

    private Node CreateNode(int start, int end)
    {
        Node node;
        if (_nodesInUse == _nodePool.Count)
        {
            node = new Node();
            _nodePool.Add(node);
        }
        else
        {
            node = _nodePool[_nodesInUse];
            node.Children.Clear();
            node.SuffixLink = null;
        }

        Debug.Assert(node.Children.Count == 0);
        Debug.Assert(node.SuffixLink is null);

        _nodesInUse++;

        node.Start = start;
        node.End = end;

        return node;
    }

    private Node InsertSplit()
    {
        Debug.Assert(_activeEdge != null);
        Node splittable = _activeEdge;

        // Insert new branch node in place of split node
        Node branch = CreateNode(splittable.Start, splittable.Start + _activeLength);
        _activeParent.Children[FirstCharOf(splittable)] = branch;

        _activeEdge = branch;
        AddSuffixLink(branch);

        // Update split node, and reinsert as child of new branch
        splittable.Start = branch.End;
        branch.Children.Add(FirstCharOf(splittable), splittable);

        return branch;
    }

    private void AddSuffixLink(Node node)
    {
        if (_needSuffixLink != null)
        {
            _needSuffixLink.SuffixLink = node;
        }

        _needSuffixLink = node;
    }

    internal class Node
    {
        public int Start, End;
        //public Node Link; >> refactored to use Dictionary<Node, Node> _suffixLinks 
        public bool IsLeaf => End == BOUNDLESS;

        public Node? SuffixLink;

        public readonly Dictionary<char, Node> Children = [];
    }

}

