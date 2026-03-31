using UnityEngine;
using TMPro;
using System.Collections;

public class Dialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public TextMeshProUGUI TextComponent;   
    public string[] lines;
    public float textSpeed = 0.05f;

    private int index = 0;

    private void Start()
    {
        
        TextComponent.text = string.Empty;
        StartDialogue();
    }

    public void StartDialogue()
    {
        index = 0;
        TextComponent.text = string.Empty;
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        TextComponent.text = string.Empty;

        foreach (char c in lines[index].ToCharArray())
        {
            TextComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            if (TextComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                // Skip typing and show full line immediately
                StopAllCoroutines();
                TextComponent.text = lines[index];
            }
        }
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            TextComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            // End of dialogue
            gameObject.SetActive(false);
        }
    }
}