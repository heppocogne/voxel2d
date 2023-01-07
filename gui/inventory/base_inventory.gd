tool
class_name BaseInventory
extends NinePatchRect

signal inventory_manipulated(items)

var player_node:Node
var world_node:Node


func _ready():
	world_node=get_tree().root.get_node("GameScreen/ViewportContainer/Viewport/World")
	player_node=world_node.get_node("Player")
	var i:=0
	var items:Array=player_node.Inventory.Items
	connect("inventory_manipulated",player_node.Inventory,"OnInventoryManipulated")
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer2.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
		s.connect("state_changed",self,"_inform_inverntory_state")
		i+=1
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
		s.connect("state_changed",self,"_inform_inverntory_state")
		i+=1


func _on_MarginContainer_resized_or_sort_children():
	rect_min_size=$MarginContainer.rect_size


func _inform_inverntory_state():
	var items:Array=[]
	var i:=0
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer2.get_children():
		if s:
			items.push_back(preload("res://gameplay/entity/item/item.tscn").instance())
			items[i].ItemTexture=s.item_texture.texture
			items[i].ItemName=s.display_name
			items[i].Quantity=s.quantity
		else:
			items[i]=null
		i+=1
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer.get_children():
		if s:
			items.push_back(preload("res://gameplay/entity/item/item.tscn").instance())
			items[i].ItemTexture=s.item_texture.texture
			items[i].ItemName=s.display_name
			items[i].Quantity=s.quantity
		else:
			items[i]=null
		i+=1
	
	emit_signal("inventory_manipulated",items)
