
public class WaypointNode
{
    public UnityEngine.Transform Waypoint;
    public WaypointNode Next;

    public WaypointNode(UnityEngine.Transform waypoint)
    {
        Waypoint = waypoint;
        Next     = null;
    }
}


public class WaypointLinkedList
{
    private WaypointNode _head;
    private WaypointNode _tail;
    private int          _count;

    public int  Count   => _count;
    public bool IsEmpty => _count == 0;

    // -----------------------------------------------------------------------
    // Add
    // -----------------------------------------------------------------------

    /// <summary>Appends a waypoint Transform to the end of the list.</summary>
    public void Append(UnityEngine.Transform waypoint)
    {
        WaypointNode newNode = new WaypointNode(waypoint);
        if (_head == null) { _head = newNode; _tail = newNode; }
        else { _tail.Next = newNode; _tail = newNode; }
        _count++;
    }

    // -----------------------------------------------------------------------
    // Access
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the waypoint at index, wrapping automatically.
    /// Enemies use this to get their current target position.
    /// </summary>
    public UnityEngine.Transform GetWaypoint(int index)
    {
        if (IsEmpty) return null;
        int safe = index % _count;
        WaypointNode cur = _head;
        for (int i = 0; i < safe; i++) cur = cur.Next;
        return cur.Waypoint;
    }

    /// <summary>
    /// Returns the next index after currentIndex, wrapping to 0 at end.
    /// Called when enemy reaches its target — gives the next waypoint index.
    /// </summary>
    public int GetNextIndex(int currentIndex)
    {
        return (currentIndex + 1) % _count;
    }

    // -----------------------------------------------------------------------
    // Debug
    // -----------------------------------------------------------------------

    public void PrintAll()
    {
        if (IsEmpty) { UnityEngine.Debug.Log("[WaypointLinkedList] Empty."); return; }
        WaypointNode cur = _head;
        int i = 0;
        while (cur != null)
        {
            UnityEngine.Debug.Log($"  [{i}] {cur.Waypoint.name} @ {cur.Waypoint.position}");
            cur = cur.Next; i++;
        }
    }
}
