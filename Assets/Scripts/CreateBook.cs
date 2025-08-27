using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBook : MonoBehaviour
{
    // 引用VRBookReader组件
    public GameObject book;

    // 外部调用此方法，传入book内容（string），显示到VRBookReader
    public void ShowBook()
    {
        if (book != null)
        {
            // 设置VRBookReader的文本内容
            book.SetActive(true);
        }
        else
        {
            Debug.LogError("未绑定Book组件！");
        }
    }
}
