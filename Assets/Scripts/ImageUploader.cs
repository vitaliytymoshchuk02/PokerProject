using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

public class ImageUploader : MonoBehaviour
{
    public Button uploadButton;
    public Image[] displayImage;
    public Vector2 maxAvatarSize = new Vector2(100, 100); // Define the maximum size for the avatar

    private void Start()
    {
        if (uploadButton != null)
        {
            uploadButton.onClick.AddListener(OpenFileBrowser);
        }

        // Load saved image if it exists
        LoadSavedImage();
    }

    private void OpenFileBrowser()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, "Select Image", "Load");

        if (FileBrowser.Success)
        {
            string[] filePaths = FileBrowser.Result;

            if (filePaths.Length > 0)
            {
                StartCoroutine(LoadImage(filePaths[0])); // Use the first file path
            }
        }
    }

    private IEnumerator LoadImage(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + filePath))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load image: " + www.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                AssignTextureToImage(texture);

                // Save the image as a file
                SaveImageToFile(texture, "avatar.png");
            }
        }
    }

    private void AssignTextureToImage(Texture2D texture)
    {
        // Create a new Sprite from the texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Assign the Sprite to the Image component
        foreach (Image image in displayImage)
        {
            image.sprite = sprite;
        }

        // Adjust the size of the Image component to fit within the max avatar size
        AdjustImageSize(texture);
    }

    private void AdjustImageSize(Texture2D texture)
    {
        float textureWidth = texture.width;
        float textureHeight = texture.height;
        float targetWidth = maxAvatarSize.x;
        float targetHeight = maxAvatarSize.y;

        float aspectRatio = textureWidth / textureHeight;
        float scaledWidth = targetHeight * aspectRatio;
        float scaledHeight = targetWidth / aspectRatio;

        if (textureWidth > targetWidth || textureHeight > targetHeight)
        {
            if (aspectRatio > 1)
            {
                foreach (Image image in displayImage)
                    image.rectTransform.sizeDelta = new Vector2(targetWidth, scaledHeight);
            }
            else
            {
                foreach (Image image in displayImage)
                    image.rectTransform.sizeDelta = new Vector2(scaledWidth, targetHeight);
            }
        }
        else
        {
            foreach (Image image in displayImage)
                image.rectTransform.sizeDelta = new Vector2(textureWidth, textureHeight);
        }
    }

    private void SaveImageToFile(Texture2D texture, string fileName)
    {
        // Encode texture to PNG
        byte[] bytes = texture.EncodeToPNG();

        // Save to persistent data path
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Saved image to: " + filePath);
    }

    private void LoadSavedImage()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "avatar.png");

        if (File.Exists(filePath))
        {
            // Load the texture from file
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);

            // Assign the loaded texture to the Image component
            AssignTextureToImage(texture);
        }
    }
}
