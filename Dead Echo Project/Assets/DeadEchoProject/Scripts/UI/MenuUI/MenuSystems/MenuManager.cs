using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MenuType { Additive, Override}
public class MenuManager : MonoBehaviour
{

    public List<MenuObject> menuObjects = new List<MenuObject>();

    // ----------------------------------------------------------------------
    // Name: Start
    // Desc: Called on the scene start, mainly this method get the system
    //       dependencies.
    // ----------------------------------------------------------------------
    void Start()
    {
        //menuObjects = GetComponents<MenuObject>().ToList();
    }

    public void OpenMenu(string menuName)
    {
        if (menuObjects.Find(e => e.menuName == menuName))
        {
            bool Overridable = menuObjects.Find(e => e.menuName == menuName).type == MenuType.Override;
            foreach (var obj in menuObjects)
            {
                if (obj.menuName == menuName) obj.OpenMenu();
                else if (Overridable) obj.CloseMenu();
            }
        }
        else Debug.LogWarning("This object is not in the object list");
    }
    public void OpenMenu(MenuObject menu)
    {
        if (menuObjects.Contains(menu))
        {
            bool Overridable = menu.type == MenuType.Override;
            foreach (var obj in menuObjects)
            {
                if (obj == menu) obj.OpenMenu();
                else if (Overridable) obj.CloseMenu();
            }
        }
        else Debug.LogWarning("This object is not in the object list");
    }
}