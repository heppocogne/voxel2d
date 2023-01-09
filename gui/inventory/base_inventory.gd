tool
class_name BaseInventory
extends PanelContainer

signal inventory_manipulated(items)

var player_node:Node
var world_node:Node


func _ready():
	world_node=get_tree().root.get_node("GameScreen/ViewportContainer/Viewport/World")
	player_node=world_node.get_node("Player")
	connect("inventory_manipulated",player_node.Inventory,"OnInventoryManipulated")
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer.get_children():
		s.connect("state_changed",self,"_inform_inverntory_state")
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer2.get_children():
		s.connect("state_changed",self,"_inform_inverntory_state")
	_update_inventory()


func _on_MarginContainer_resized_or_sort_children():
	rect_min_size=$MarginContainer.rect_size


func _update_inventory():
	var i:=0
	var items:Array=player_node.Inventory.Items
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer2.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
		else:
			s.clear()
		i+=1
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
		else:
			s.clear()
		i+=1


func _inform_inverntory_state():
	var items:Array=[]

	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer2.get_children():
		if s and s.display_name!="":
			items.push_back(world_node.CreateItemInstance(s.display_name,s.quantity))
		else:
			items.push_back(null)
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer.get_children():
		if s and s.display_name!="":
			items.push_back(world_node.CreateItemInstance(s.display_name,s.quantity))
		else:
			items.push_back(null)
			
	emit_signal("inventory_manipulated",items)
