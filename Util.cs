using Godot;
using System;
using System.Collections.Generic;

public static class Util {
    public static T FirstOrDefaultNodeOfType<T>(this Node root) where T : Node
    {
        if (root == null) return null;

        Queue<Node> queue = [];
        queue.Enqueue(root);

        while (queue.TryDequeue(out var current))
        {
            if (current is T component)
                return component;
            
            foreach (Node child in current.GetChildren())
                queue.Enqueue(child);
        }
        return null; 
    }
}
