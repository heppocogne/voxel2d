extends BaseInventory

signal utility_inventory_manipulated(items)
signal burn_started(utility)
signal burn_ended(utility)

var utility_node:Node
var slots:Array
onready var fuel_timer:Timer=$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/Inputs/TextureProgress/FuelTimer
onready var fuel_progress:TextureProgress=$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/Inputs/TextureProgress
onready var burn_timer:Timer=$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/TextureProgress/BurnTimer
onready var burn_progress:TextureProgress=$MarginContainer/VBoxContainer/CenterContainer/FurnaceInventory/TextureProgress

func _ready():
	connect("utility_inventory_manipulated",utility_node.Inventory,"OnInventoryManipulated")
	for i in range(3):
		slots[i].connect("state_changed",self,"_inform_inverntory_state")
	
	slots[0].connect("state_changed",self,"_on_burned_changed")
	slots[1].connect("state_changed",self,"_on_fuel_changed")


func _process(delta:float):
	fuel_progress.value=fuel_timer.time_left
	if 0<fuel_timer.time_left:
		burn_progress.value=burn_timer.wait_time-burn_timer.time_left
	else:
		burn_progress.value=max(0,burn_progress.value-delta*2)


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


func _on_fuel_changed():
	var fuel_data:Dictionary=world_node.FindFuelData(slots[1].display_name)
	if !fuel_data.empty():
		if 0<fuel_timer.time_left:
			return
		slots[1].set_iten_quantity(slots[1].quantity-1)
		if slots[1].quantity==0:
			slots[1].clear()
			emit_signal("burn_ended",utility_node)
			return
		var t:float=fuel_data["Energy"]
		fuel_timer.start(t)
		emit_signal("burn_started",utility_node)
		fuel_progress.max_value=t
		fuel_progress.value=t


func _on_burned_changed():
	var recipe:Dictionary=world_node.FindFurnaceRecipe(slots[0].display_name)
	if !recipe.empty():
		if (
			0<fuel_timer.time_left
			and (slots[2].display_name=="" or slots[2].display_name==recipe["Output"])
			and slots[2].quantity<world_node.FindItemData(recipe["Output"])["Stack"]
		):
			burn_timer.start()
		else:
			burn_timer.stop()


func _inform_inverntory_state():
	var items:Array=[]
	for s in slots:
		if s and s.display_name!="":
			items.push_back(world_node.CreateItemInstance(s.display_name,s.quantity))
		else:
			items.push_back(null)
	emit_signal("utility_inventory_manipulated",items)
	._inform_inverntory_state()


func _on_FuelTimer_timeout():
	_on_fuel_changed()


func _on_BurnTimer_timeout():
	var recipe:Dictionary=world_node.FindFurnaceRecipe(slots[0].display_name)
	slots[2].set_item_quantity(slots[2].quantity+1)
	slots[0].set_item_quantity(slots[0].quantity-1)
	if slots[0].quantity==0:
		slots[0].clear()
		return
	burn_timer.start()
