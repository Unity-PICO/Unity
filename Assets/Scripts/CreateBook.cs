using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBook : MonoBehaviour
{
    // ����VRBookReader���
    public GameObject book;

    // �ⲿ���ô˷���������book���ݣ�string������ʾ��VRBookReader
    public void ShowBook()
    {
        if (book != null)
        {
            // ����VRBookReader���ı�����
            book.SetActive(true);
        }
        else
        {
            Debug.LogError("δ��Book�����");
        }
    }
}
