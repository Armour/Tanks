using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	
    public int m_NumRoundsToWin = 3;        
    public float m_StartDelay = 1.5f;         
	public float m_EndDelay = 2.0f;    
	public bool duelMode;        
    public CameraControl m_CameraControl;   
    public Text m_MessageText;              
    public GameObject m_TankPrefab;         
	public TankManager[] m_Tanks;
	public TankManager[] m_duelTanks;


    private int m_RoundNumber;                
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;


    private void Start() {
        SpawnAllTanks();
        SetCameraTargets();
        StartCoroutine(GameLoop());
    }
		
    private void SpawnAllTanks() {
        for (int i = 0; i < m_Tanks.Length; i++) {
            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
		if (duelMode) {
			for (int i = 0; i < m_duelTanks.Length; i++) {
				m_duelTanks[i].m_PlayerColor = m_Tanks[i].m_PlayerColor;
				m_duelTanks[i].m_Instance = Instantiate(m_TankPrefab, m_duelTanks[i].m_SpawnPoint.position, m_duelTanks[i].m_SpawnPoint.rotation) as GameObject;
				m_duelTanks[i].m_PlayerNumber = i + 1;
				m_duelTanks[i].Setup();
			}
		}
    }

    private void SetCameraTargets() {
		Transform[] targets;
		if (!duelMode) {
			targets = new Transform[m_Tanks.Length];
			for (int i = 0; i < targets.Length; i++) {
				targets[i] = m_Tanks[i].m_Instance.transform;
			}
		} else {
			targets = new Transform[m_Tanks.Length * 2];
			for (int i = 0; i < m_Tanks.Length; i++) {
				targets[i] = m_Tanks[i].m_Instance.transform;
				targets[i + m_duelTanks.Length] = m_duelTanks[i].m_Instance.transform;
			}
		}
        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop() {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null) {
            SceneManager.LoadScene(0);
        } else {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting() {
		ResetAllTanks();
		DisableTankControl();
		m_CameraControl.SetStartPositionAndSize();
		m_RoundNumber++;
		m_MessageText.text = "ROUND " + m_RoundNumber;
		yield return new WaitForSeconds(m_StartDelay);
    }

    private IEnumerator RoundPlaying() {
		EnableTankControl();
		m_MessageText.text = string.Empty;
		while (!OneTankLeft()) {
			yield return null;
		};
    }

    private IEnumerator RoundEnding() {
		DisableTankControl();
		m_RoundWinner = null;
		m_RoundWinner = GetRoundWinner();
		if (m_RoundWinner != null)
			m_RoundWinner.m_Wins++;
		m_GameWinner = GetGameWinner();
		m_MessageText.text = EndMessage();
		yield return new WaitForSeconds(m_EndDelay);
    }
		
    private bool OneTankLeft() {
		int numTanksLeft = m_Tanks.Length;

		if (!duelMode) {
	        for (int i = 0; i < m_Tanks.Length; i++) {
	            if (!m_Tanks[i].m_Instance.activeSelf)
	                numTanksLeft--;
	        }
		} else {
			for (int i = 0; i < m_Tanks.Length; i++) {
				if (!m_Tanks[i].m_Instance.activeSelf && !m_duelTanks[i].m_Instance.activeSelf)
					numTanksLeft--;
			}
		}

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner() {
        for (int i = 0; i < m_Tanks.Length; i++) {
			if (m_Tanks[i].m_Instance.activeSelf || (duelMode && m_duelTanks[i].m_Instance.activeSelf))
                return m_Tanks[i];
        } 
        return null;
    }

    private TankManager GetGameWinner() {
        for (int i = 0; i < m_Tanks.Length; i++) {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }
        return null;
    }

    private string EndMessage() {
        string message = "ROUND END!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++) {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }

    private void ResetAllTanks() {
        for (int i = 0; i < m_Tanks.Length; i++) {
            m_Tanks[i].Reset();
        }
		if (duelMode) {
			for (int i = 0; i < m_duelTanks.Length; i++) {
				m_duelTanks[i].Reset();
			}
		}
    }
		
    private void EnableTankControl() {
        for (int i = 0; i < m_Tanks.Length; i++) {
            m_Tanks[i].EnableControl();
        }
		if (duelMode) {
			for (int i = 0; i < m_duelTanks.Length; i++) {
				m_duelTanks[i].EnableControl();
			}
		}
    }

    private void DisableTankControl() {
		for (int i = 0; i < m_Tanks.Length; i++) {
            m_Tanks[i].DisableControl();
        }
		if (duelMode) {
			for (int i = 0; i < m_duelTanks.Length; i++) {
				m_duelTanks[i].DisableControl();
			}
		}
    }
}