# adding a help target so the default target is a no-op
.PHONY: help
help:
	@echo "Please use \`make <target>' where <target> is one of"
	@echo "  bootstrap    to install the required tools"
	@echo "  generate     to generate the bindings"

.PHONY: generate
generate: $(wit_files)
	wit-bindgen c --export wit/ephemeral/spin-http.wit --out-dir ./src/Fermyon.Spin.Sdk/native/wit/
	wit-bindgen c --import wit/ephemeral/wasi-outbound-http.wit --out-dir ./src/Fermyon.Spin.Sdk/native/wit/
	wit-bindgen c --import wit/ephemeral/outbound-redis.wit --out-dir ./src/Fermyon.Spin.Sdk/native/wit/
	wit-bindgen c --import wit/ephemeral/outbound-pg.wit --out-dir ./src/Fermyon.Spin.Sdk/native/wit/
	wit-bindgen c --import wit/ephemeral/spin-config.wit --out-dir ./src/Fermyon.Spin.Sdk/native/wit/

.PHONY: bootstrap
bootstrap:
	# install the WIT Bindgen version we are currently using in Spin e06c6b1
	cargo install wit-bindgen-cli --force --git https://github.com/bytecodealliance/wit-bindgen --tag v0.2.0
	cargo install wizer --all-features
