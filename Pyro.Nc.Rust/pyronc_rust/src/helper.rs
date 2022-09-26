

pub  trait Receiver{
    fn on_receive(&self);
    fn on_receive_message(&self, message: &str);
}

pub  struct Handler{
    pub receivers: Vec<Box<dyn Receiver>>
}

impl Handler {
    pub fn register(&mut self, rec: Box<dyn Receiver>) -> &mut Self{ 
        self.receivers.push(rec);
        return self;
    }
    pub fn notify(self){
        let iterator = self.receivers.iter();
        for item in iterator {
            item.on_receive();
        }
    }

    pub fn notify_message(&self, message: &str){
        let iterator = self.receivers.iter();
        for item in iterator {
            item.on_receive_message(message);
        }
    }
}



pub  struct Class1{
    
}

impl Receiver for Class1 {
    fn on_receive(&self) {
        self.on_receive_message("Called class1");
    }

    fn on_receive_message(&self, message: &str) {
        println!("{}", message);
    }
}

pub  struct Class2{
    
}

impl Receiver for Class2 {
    fn on_receive(&self) {
        self.on_receive_message("Called class2");
    }

    fn on_receive_message(&self, message: &str) {
        println!("{}", message);
    }
}


