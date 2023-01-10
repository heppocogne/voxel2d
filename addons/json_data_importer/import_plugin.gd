extends EditorImportPlugin


func get_importer_name()->String:
	return "heppocogne.godot-json-importer"


func get_visible_name()->String:
	return "JSON Data"


func get_import_options(preset:int)->Array:
	return []


func get_recognized_extensions()->Array:
	return ["json"]


func get_save_extension()->String:
	return "res"


func get_resource_type()->String:
	return "Resource"


func get_preset_count()->int:
	return 1

func get_preset_name(i)->String:
	return "Default"


func import(source_file:String,save_path:String,options:Dictionary,platform_variants:Array,gen_files:Array)->int:
	var f:=File.new()
	var err:=f.open(source_file,File.READ)
	if err==OK:
		var parse_result:=JSON.parse(f.get_as_text())
		if parse_result.error==OK:
			var res:Resource=preload("res://addons/json_data_importer/json_data.gd").new()
			res.data=parse_result.result
			err=ResourceSaver.save(save_path+"."+get_save_extension(),res)
			if err!=OK:
				push_error("cannot save resource (error code:"+str(err)+")")
			return err
		else:
			push_error(parse_result.error_string+" at line "+str(parse_result.error_line))
			return parse_result.error
	push_error("cannot open file (error code:"+str(err)+")")
	return err
