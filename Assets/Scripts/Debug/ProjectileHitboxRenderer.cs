using System.Collections;
using UnityEngine;

/// <summary>
/// Attempts to render hitboxes for projectiles. Debug functionality attached to the Battle scene's camera.
/// </summary>
public class ProjectileHitboxRenderer : MonoBehaviour
{
    private Projectile[] projectiles;

    private GameObject root;

    private Vector3 topLeft;
    private Vector3 topRight;
    private Vector3 bottomLeft;
    private Vector3 bottomRight;
    private int zIndex = -9;
    private Shader shdr;
    private Material mat;

    private void Start()
    {
        root = GameObject.Find("Canvas");
        shdr = Shader.Find("Sprites/Default");
        mat = new Material(shdr);
    }

    private IEnumerator OnPostRender()
    {
        yield return new WaitForEndOfFrame(); // need to wait for UI to finish drawing first, or it'll appear under the UI
        // note: it kinda still appears under the UI due to its rendering settings
        projectiles = root.GetComponentsInChildren<Projectile>();
        foreach (Projectile p in projectiles)
        {
            topLeft = p.self.position;
            topLeft.Set(topLeft.x - p.self.rect.width / 2, topLeft.y + p.self.rect.height / 2, zIndex);

            bottomRight = p.self.position;
            bottomRight.Set(bottomRight.x + p.self.rect.width/2, bottomRight.y - p.self.rect.height/2, zIndex);

            bottomLeft = p.self.position;
            bottomLeft.Set(bottomLeft.x - p.self.rect.width / 2, bottomLeft.y - p.self.rect.height / 2, zIndex);

            topRight = p.self.position;
            topRight.Set(topRight.x + p.self.rect.width / 2, topRight.y + p.self.rect.height / 2, zIndex);

            topLeft.Set(topLeft.x / Screen.width, topLeft.y / Screen.height, zIndex);
            topRight.Set(topRight.x / Screen.width, topRight.y / Screen.height, zIndex);
            bottomLeft.Set(bottomLeft.x / Screen.width, bottomLeft.y / Screen.height, zIndex);
            bottomRight.Set(bottomRight.x / Screen.width, bottomRight.y / Screen.height, zIndex);

            // draw boxes
            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadOrtho();
            //GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            GL.Color(Color.magenta);

            GL.Vertex(topLeft);
            GL.Vertex(topRight);

            GL.Vertex(topRight);
            GL.Vertex(bottomRight);

            GL.Vertex(bottomRight);
            GL.Vertex(bottomLeft);

            GL.Vertex(bottomLeft);
            GL.Vertex(topLeft);
            GL.End();
            GL.PopMatrix();
        }
    }
}