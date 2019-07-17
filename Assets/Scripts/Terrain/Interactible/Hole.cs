using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Hole : Interactible {
    
    protected override void Behaviour()
    {
        base.Behaviour();

        if (DataManager.instance.isMulti)
        {
            CmdExpansionBehaviour();
        }
        else
        {
            LocalExpansionBehaviour();
        }
        tile.holeCoroutine = StartCoroutine(tile.HoleBehaviour());
        SoundManager.instance.PlaySound(51, 1.0f, AudioType.SFX);
    }


    ///////////// PRIVATE LOCAL FUNCTIONS //////////////
    private void LocalExpansionBehaviour()
    {
        foreach (Tile t in tile.GetNeighbourOfType(Tile.TileType.Hole))
        {
            if (DataManager.instance.isMulti)
            {
                RpcBehaviour(t.gameObject);
            }
            else
            {
                LocalActivationBehaviour(t.gameObject);
            }
        }
    }
    
    private void LocalActivationBehaviour(GameObject _t)
    {
        if (!_t.GetComponent<Tile>().isActive)
        {
            _t.GetComponent<Tile>().Activate(0.2f);
        }
    }


    ///////////// PRIVATE NETWORK FUNCTIONS //////////////

    [Command]
    private void CmdExpansionBehaviour()
    {
        LocalExpansionBehaviour();
    }

    [ClientRpc]
    private void RpcBehaviour(GameObject _t)
    {
        LocalActivationBehaviour(_t);
    }
}
