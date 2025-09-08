using UnityEngine;
using UnityEngine.UI;

public class ButtonClickListener : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnClick()
    {
        if(FolderFinder.finder != null) {
            FolderFinder.finder.OnClick(GetComponentInChildren<Text>());
            Debug.Log("Button Clicked");
            } 
    }
}
