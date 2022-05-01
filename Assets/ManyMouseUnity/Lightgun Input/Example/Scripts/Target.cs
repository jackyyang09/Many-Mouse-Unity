using UnityEngine;

        
public class Target : BaseTarget {
    public Collider bullsEye;
    public int health = 1;
    public override void Shot (RaycastHit hitInfo) {
        if(hitInfo.collider == bullsEye) {
            Game.Instance.score += 150 * Game.Instance.multiplier;
            Object.Instantiate<GainScoreUI>(Game.Instance.gainScoreUIPrefab, transform.position, Quaternion.identity, Game.Instance.canvas.transform).Init(transform.position, 150, Game.Instance.multiplier);
        } else {
            Game.Instance.score += 50 * Game.Instance.multiplier;
            Object.Instantiate<GainScoreUI>(Game.Instance.gainScoreUIPrefab, transform.position, Quaternion.identity, Game.Instance.canvas.transform).Init(transform.position, 50, Game.Instance.multiplier);
        }
        Game.Instance.multiplier += 1;
        health--;
        if(health <= 0)
            Destroy(gameObject);
    }
}
public class BaseTarget : MonoBehaviour {
    public MovementStyle movementStyle;
    // public Vector3 ;
    public virtual void Shot (RaycastHit hitInfo) {
    }
}
