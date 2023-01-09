extends BaseInventory

var craft_list:VBoxContainer


func _ready():
	craft_list=$MarginContainer/VBoxContainer/HBoxContainer/ScrollContainer/VBoxContainer
	_update_recipe_list()


func _update_recipe_list():
	var available:Dictionary=player_node.Inventory.GetAvailableRecipes()
	for r in available:
		var cb:CraftButton=preload("res://gui/inventory/default_inventory/craft_button.tscn").instance()
		cb.set_recipe(available[r],r)
		craft_list.add_child(cb)
		cb.connect("crafted",self,"_on_crafted")


func _on_crafted(_result,_consumed):
	while craft_list.get_child_count()!=0:
		craft_list.remove_child(craft_list.get_child(craft_list.get_child_count()-1))
	_update_inventory()
	_update_recipe_list()
