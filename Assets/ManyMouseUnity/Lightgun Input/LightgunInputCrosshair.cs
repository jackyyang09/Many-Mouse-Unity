using UnityEngine;
using UnityEngine.UI;
using LightgunEngine;

[RequireComponent(typeof(RectTransform))]
public class LightgunInputCrosshair : MonoBehaviour
{
    RectTransform rectTransform => (RectTransform)transform;
    public Image crosshairImage;
    public Image hitImage;
    public bool usingMouseInput;
    public Lightgun lightgun;
    // public TMPro.TextMeshProUGUI coordText;
    public AudioClip shootSFX;
    public AudioClip hitSFX;
    public GameObject shootVFX;
    public GameObject hitVFX;

    public void Initialize(Lightgun lightgun)
    {
        this.lightgun = lightgun;
        usingMouseInput = lightgun == null;
        hitImage.CrossFadeAlpha(0, 0, true);
        Update();
    }

    void Update()
    {
        crosshairImage.color = hitImage.color = usingMouseInput ? Color.white : lightgun.index == 0 ? Color.red : Color.cyan;

        var position = usingMouseInput ? (Vector2)Input.mousePosition : lightgun.Position;
        RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)rectTransform.parent, position, rectTransform.GetComponentInParent<Canvas>().rootCanvas.worldCamera, out Vector3 worldPoint);
        rectTransform.position = worldPoint;

        // coordText.text = string.Format("{0:F0}", position.x)+", "+string.Format("{0:F0}", position.y);
        // coordText.text = string.Format("{0:0.00}", lightgun.viewportPosition.x)+", "+string.Format("{0:0.00}", lightgun.viewportPosition.y);

        var fire = usingMouseInput ? Input.GetMouseButtonDown(0) : lightgun.GetButtonDown(0);
        if (fire)
        {
            Game.Instance.screenFlash.enabled = true;
            Game.Instance.screenFlash.CrossFadeAlpha(1f, 0, true);
            Game.Instance.screenFlash.CrossFadeAlpha(0, 0.1f, true);
            var ray = Camera.main.ScreenPointToRay(position);
            AudioSource.PlayClipAtPoint(shootSFX, Camera.main.transform.position, 0.4f);
            Object.Instantiate(shootVFX, rectTransform.position, Quaternion.AngleAxis(Random.value * 360, rectTransform.forward), transform.parent);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000000))
            {
                AudioSource.PlayClipAtPoint(hitSFX, Camera.main.transform.position, 1f);
                Object.Instantiate(hitVFX, rectTransform.position, Quaternion.AngleAxis(Random.value * 360, rectTransform.forward), transform.parent);
                hitImage.CrossFadeAlpha(1, 0, true);
                hitImage.CrossFadeAlpha(0, 0.5f, false);
                hitInfo.transform.SendMessageUpwards("Shot", hitInfo, SendMessageOptions.DontRequireReceiver);
                // hitInfo.rigidbody.AddForce(ray.direction * force);
                // Object.Instantiate<ParticleSystem>(particleSystemPrefab, hitInfo.point, Quaternion.identity);
            }
            else
            {
                Game.Instance.multiplier = 1;
            }
        }
    }
}