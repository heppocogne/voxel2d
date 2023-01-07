tool
class_name BaseInventory
extends NinePatchRect

var player_node:Node
var world_node:Node


func _ready():
	world_node=get_tree().root.get_node("GameScreen/ViewportContainer/Viewport/World")
	player_node=world_node.get_node("Player")
	var i:=0
	var items:Array=player_node.Inventory.Items
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer2.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
			i+=1
	for s in $MarginContainer/VBoxContainer/PlayerInventory/GridContainer.get_children():
		if items[i]!=null:
			s.set_item_info(items[i].ItemName,items[i].ItemTexture,items[i].Quantity)
			i+=1


func _on_MarginContainer_resized_or_sort_children():
	rect_min_size=$MarginContainer.rect_size
