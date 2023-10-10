.PHONY: generate
generate:
	# wit-bindgen c --export wit/ephemeral/spin-http.wit --out-dir ./src/native/
	# wit-bindgen c --import wit/ephemeral/wasi-outbound-http.wit --out-dir ./src/native/
	# wit-bindgen c --import wit/ephemeral/outbound-redis.wit --out-dir ./src/native
	# wit-bindgen c --import wit/ephemeral/outbound-pg.wit --out-dir ./src/native
	# wit-bindgen c --import wit/ephemeral/spin-config.wit --out-dir ./src/native

.PHONY: bootstrap
bootstrap:
	# install the WIT Bindgen version we are currently using in Spin e06c6b1
	cargo install wit-bindgen-cli --git https://github.com/bytecodealliance/wit-bindgen --rev cb871cfa1ee460b51eb1d144b175b9aab9c50aba --force
	cargo install wizer --all-features

.PHONY: clean
clean:
	find . -type d -name bin | xargs rm -rf
	find . -type d -name obj | xargs rm -rf
