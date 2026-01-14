using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerFacingHandler : NetworkBehaviour
{
    private Player player;
    private Rigidbody2D rb;
    private bool isOnLeft;
    private bool waitingForFlip;
    private bool facingRight = true;

    void Start()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsOwner) return;
        HandleSideCheck();
    }

    private void HandleSideCheck()
    {
        var enemy = player.GetEnemyPlayer();
        if (enemy == null) return;

        bool currentlyOnLeft = transform.position.x < enemy.transform.position.x;

        // When the player crosses sides with the opponent
        if (currentlyOnLeft != isOnLeft && !waitingForFlip)
        {
            isOnLeft = currentlyOnLeft;
            StartCoroutine(WaitForFlipInput(currentlyOnLeft));
        }
    }

    private IEnumerator WaitForFlipInput(bool sideAtCross)
    {
        waitingForFlip = true;
        float timeout = 1.5f;
        float elapsed = 0f;

        while (true)
        {
            float moveInput = SimpleInput.GetAxis("Horizontal");

            // If on the left, pressing right means toward enemy
            if (sideAtCross && moveInput > 0)
            {
                SetFacing(true);
                RequestFlipServerRpc(true);
                break;
            }
            // If on the right, pressing left means toward enemy
            else if (!sideAtCross && moveInput < 0)
            {
                SetFacing(false);
                RequestFlipServerRpc(false);
                break;
            }

            // Auto timeout to force correct facing
            elapsed += Time.deltaTime;
            if (elapsed > timeout)
            {
                SetFacing(sideAtCross);
                RequestFlipServerRpc(sideAtCross);
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        waitingForFlip = false;
    }
    private void SetFacing(bool faceRight)
    {
        if (facingRight == faceRight) return;
        facingRight = faceRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1 : -1);
        transform.localScale = scale;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestFlipServerRpc(bool faceRight)
    {
        FlipClientRpc(faceRight);
    }

    [ClientRpc]
    private void FlipClientRpc(bool faceRight)
    {
        SetFacing(faceRight);
    }
}
