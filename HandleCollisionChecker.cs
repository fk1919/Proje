using UnityEngine;

public class HandleCollisionChecker : MonoBehaviour
{
    public MissionManager missionManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("C�r�k"))
        {
            Debug.Log("Ayna sap� ��r��e temas etti!");

            // G�rev tamamlan�yor
            if (missionManager.GetCurrentTask() == 3) // 4. g�rev
            {
                missionManager.CompleteCurrentTask();
            }
        }
    }
}