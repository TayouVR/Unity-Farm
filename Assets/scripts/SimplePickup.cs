
public class SimplePickup : Interactable {

	public bool isAmmo;
	public AmmoType ammoObject;
	public int amount;

	public int PickUp() {
		Destroy(gameObject);
		return amount;
	}
}