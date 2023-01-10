extends BaseInventory

signal utility_inventory_manipulated(items)

var utility_node:Node
var slots:Array


func _ready():
	connect("utility_inventory_manipulated",utility_node.Inventory,"OnInventoryManipulated")
	for i in range(3):
		slots[i].connect("state_changed",self,"_inform_inverntory_state")


func _update_inventory():
	._update_inventory()
	
	slots=[
		$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/Inputs/InventorySlot,
		$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/Inputs/InventorySlot2,
		$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/InventorySlot3,
	]
	
	var i:=0
	var items:Array=utility_node.Inventory.Items
	for s in slots:
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
		else:
			s.clear()
		i+=1


func _inform_inverntory_state():
	var items:Array=[]
	for s in slots:
		if s and s.display_name!="":
			items.push_back(world_node.CreateItemInstance(s.display_name,s.quantity))
		else:
			items.push_back(null)
	emit_signal("utility_inventory_manipulated",items)
	._inform_inverntory_state()
