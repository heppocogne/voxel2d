tool
class_name CraftButton
extends Button

signal crafted(item,consumed)

var recipe:Array
var result


func _ready():
	var inventory_node:Node=get_tree().root.get_node("GameScreen/ViewportContainer/Viewport/World/Player/Inventory")
	connect("crafted",inventory_node,"OnCrafted")


func set_recipe(input_items:Array,output_item):
	recipe=input_items
	var hbox:HBoxContainer=$MarginContainer/HBoxContainer
	for r in recipe:
		var material:CraftItemView=preload("res://gui/default_inventory/craft_item_view.tscn").instance()
		hbox.add_child(material)
		hbox.move_child(material,0)
		material.set_item_info(r.ItemName,r.ItemTexture,r.Quantity)
	
	result=output_item
	$MarginContainer/HBoxContainer/ResultItemView.set_item_info(
		output_item.ItemName,
		output_item.ItemTexture,
		output_item.Quantity)


func _on_MarginContainer_resized_or_sort_children():
	rect_min_size=$MarginContainer.rect_size


func _on_CraftButton_pressed():
	emit_signal("crafted",result,recipe)
