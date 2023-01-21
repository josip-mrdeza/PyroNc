use std::collections::HashMap;
use std::ptr::null;
use crate::parser::block::Block;

mod block;

pub static MAP: Vec<String> = init_dictionary();

fn init_dictionary() -> Vec<String>{
	let mut map: Vec<String> = Vec::new();
	map.push("G00".to_string());
	map.push("G01".to_string());
	map.push("G02".to_string());
	map.push("G03".to_string());
	map.push("G04".to_string());
	map.push("G05".to_string());
	map.push("G09".to_string());
	map.push("G40", )
	return map;
}

fn parse_line(line_text: String) -> Vec<Block> {
	
}
