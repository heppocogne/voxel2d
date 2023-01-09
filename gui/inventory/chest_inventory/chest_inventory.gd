extends BaseInventory

signal utility_inventory_manipulated(items)

var utility_node:Node


func _ready():
	connect("utility_inventory_manipulated",utility_node.Inventory,"OnInventoryManipulated")
	for s in $MarginContainer/VBoxContainer/ChestInventory/GridContainer.get_children():
		s.connect("state_changed",self,"_inform_inverntory_state")


func _update_inventory():
	._update_inventory()
	var i:=0
	var items:Array=utility_node.Inventory.Items
	for s in $MarginContainer/VBoxContainer/ChestInventory/GridContainer.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
		else:
			s.clear()
		i+=1


func _inform_inverntory_state():
	var items:Array=[]
	for s in $MarginContainer/VBoxContainer/ChestInventory/GridContainer.get_children():
		if s and s.display_name!="":
			items.push_back(world_node.CreateItemInstance(s.display_name,s.quantity))
		else:
			items.push_back(null)
			
	emit_signal("utility_inventory_manipulated",items)
	._inform_inverntory_state()
