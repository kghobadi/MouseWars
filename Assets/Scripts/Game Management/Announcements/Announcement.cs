using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Announcement : MonoBehaviour
{
    protected Announcer _announcer;
    
    public virtual void SetAnnouncer(Announcer announcer)
    {
        _announcer = announcer;
    }
}
